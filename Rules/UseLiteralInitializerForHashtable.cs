//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class UseLiteralInitializerForHashtable : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> diagnosticRecords;
        private HashSet<string> presetTypeNameSet;
        private string fileName;

        public UseLiteralInitializerForHashtable()
        {
            var presetTypeNames = new string[]
            {
                "system.collection.hashtable",
                "hashtable"
            };
            presetTypeNameSet = new HashSet<string>(presetTypeNames, StringComparer.OrdinalIgnoreCase);
            diagnosticRecords = new List<DiagnosticRecord>();
        }

        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }
            this.fileName = fileName;
            diagnosticRecords.Clear();
            ast.Visit(this);
            return diagnosticRecords;
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (commandAst == null
                || commandAst.CommandElements.Count < 2)
            {
                return AstVisitAction.SkipChildren;
            }

            var commandName = commandAst.GetCommandName();
            if (commandName == null
                || !commandName.Equals("new-object", StringComparison.OrdinalIgnoreCase))
            {
                return AstVisitAction.Continue;
            }
            AnalyzeNewObjectCommand(commandAst);
            return AstVisitAction.Continue;
        }

        private void AnalyzeNewObjectCommand(CommandAst commandAst)
        {
            string typeName;
            List<string> argumentList;
            GetParametersFromCommandAst(commandAst, out typeName, out argumentList);
            if (typeName == null
                || !presetTypeNameSet.Contains(typeName))
            {
                return;
            }

            if (argumentList != null)
            {
                if (argumentList.Any(arg => arg != null && arg.EndsWith("ignorecase", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
            }
            var dr = new DiagnosticRecord(
                Strings.UseLiteralInitilializerForHashtableDescription,
                commandAst.Extent,
                GetName(),
                GetDiagnosticSeverity(),
                fileName,
                ruleId: null,
                suggestedCorrections: GetSuggestedCorrections(commandAst, this.fileName));
            diagnosticRecords.Add(dr);
        }

        private void GetParametersFromCommandAst(CommandAst commandAst, out string typeName, out List<string> argumentList)
        {
            // This should read the command in all the following form
            // new-object hashtable
            // new-object -Typename hashtable
            // new-object hashtable -ArgumentList comparer
            // new-object -Typename hashtable -ArgumentList blah1,blah2
            // new-object -ArgumentList blah1,blah2 -typename hashtable

            argumentList = null;
            typeName = null;
            var namedArguments = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            namedArguments.Add("typename", null);
            namedArguments.Add("argumentlist", null);
            var positinalElems = GetNamedArguments(commandAst.CommandElements, ref namedArguments);
            GetPositionalArguments(new ReadOnlyCollection<CommandElementAst> (positinalElems), ref namedArguments);
            if (namedArguments["typename"] == null)
            {
                return;
            }

            var typenameAst = namedArguments["typename"] as StringConstantExpressionAst;
            if (typenameAst == null)
            {
                return;
            }

            typeName = typenameAst.Value;
            var argumentListAst = namedArguments["argumentlist"] as ExpressionAst;
            if (argumentListAst == null)
            {
                return;
            }

            argumentList = new List<string>(Helper.Instance.GetStringsFromExpressionAst(argumentListAst));
        }

        private int GetFirstEmptyIndex(OrderedDictionary namedArguments)
        {
            for (int k = 0; k < namedArguments.Count; k++)
            {
                if (namedArguments[k] == null)
                {
                    return k;
                }
            }
            return -1;
        }

        private void GetPositionalArguments(ReadOnlyCollection<CommandElementAst> positinalArguments, ref OrderedDictionary namedArguments)
        {
            for (int k = 0; k < positinalArguments.Count; k++)
            {
                int firstEmptyIndex = GetFirstEmptyIndex(namedArguments);
                if (firstEmptyIndex == -1)
                {
                    return;
                }
                var elem = positinalArguments[k];
                namedArguments[firstEmptyIndex] = elem as ExpressionAst;
            }
        }

        private List<CommandElementAst> GetNamedArguments(ReadOnlyCollection<CommandElementAst> commandElements, ref OrderedDictionary namedArguments)
        {
            bool paramFound = false;
            string paramName = null;
            var remainingCommandElements = new List<CommandElementAst>();
            for (int k = 1; k < commandElements.Count; k++)
            {
                if (paramFound)
                {
                    paramFound = false;
                    var argAst = commandElements[k] as ExpressionAst;
                    if (argAst != null)
                    {
                        namedArguments[paramName] = argAst;
                        continue;
                    }
                }
                var paramAst = commandElements[k] as CommandParameterAst;
                if (paramAst != null)
                {
                    foreach (var key in namedArguments.Keys)
                    {
                        var keyStr = key as string;
                        if (keyStr.Equals(paramAst.ParameterName, StringComparison.OrdinalIgnoreCase))
                        {
                            paramFound = true;
                            paramName = paramAst.ParameterName;
                            break;
                        }
                    }
                }
                else
                {
                    remainingCommandElements.Add(commandElements[k]);
                }
            }
            return remainingCommandElements;
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            if (methodCallAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            var typeExprAst = methodCallAst.Expression as TypeExpressionAst;
            if (typeExprAst == null
                || !presetTypeNameSet.Contains(typeExprAst.TypeName.FullName))
            {
                return AstVisitAction.Continue;
            }

            var memberStringConstantExprAst = methodCallAst.Member as StringConstantExpressionAst;
            if (memberStringConstantExprAst == null
                || !memberStringConstantExprAst.Value.Equals("new", StringComparison.OrdinalIgnoreCase))
            {
                return AstVisitAction.Continue;
            }

            // no arguments provided to new OR one of the argument ends with ignorecase
            // (heuristics find to something like [system.stringcomparer]::ordinalignorecase)
            if (methodCallAst.Arguments == null
                || !HasIgnoreCaseComparerArg(methodCallAst.Arguments))
            {

                var dr = new DiagnosticRecord(
                    Strings.UseLiteralInitilializerForHashtableDescription,
                    methodCallAst.Extent,
                    GetName(),
                    GetDiagnosticSeverity(),
                    fileName,
                    ruleId: null,
                    suggestedCorrections: GetSuggestedCorrections(methodCallAst, this.fileName));
                diagnosticRecords.Add(dr);
            }

            return AstVisitAction.Continue;
        }

        private bool HasIgnoreCaseComparerArg(ReadOnlyCollection<ExpressionAst> arguments)
        {
            foreach (var arg in arguments)
            {
                var memberExprAst = arg as MemberExpressionAst;
                if (memberExprAst == null)
                {
                    continue;
                }
                var strConstExprAst = memberExprAst.Member as StringConstantExpressionAst;
                if (strConstExprAst == null)
                {
                    continue;
                }
                if (strConstExprAst.Value.EndsWith("ignorecase"))
                {
                    return true;
                }
            }
            return false;
        }

        private List<CorrectionExtent> GetSuggestedCorrections(Ast violation, string filename)
        {
            var correctionExtents = new List<CorrectionExtent>();
            correctionExtents.Add(new CorrectionExtent(
                violation.Extent.StartLineNumber,
                violation.Extent.EndLineNumber,
                violation.Extent.StartColumnNumber,
                violation.Extent.EndColumnNumber,
                "@{}",
                filename,
                GetDescription()));
            return correctionExtents;
        }

        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseLiteralInitilializerForHashtableCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseLiteralInitilializerForHashtableDescription);
        }

        public string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseLiteralInitilializerForHashtableName);
        }

        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        private DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
