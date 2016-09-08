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
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

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
                "collection.hashtable",
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

            if (!commandAst.GetCommandName().Equals("new-object", StringComparison.OrdinalIgnoreCase))
            {
                return AstVisitAction.Continue;
            }
            AnalyzeNewObjectCommand(commandAst);
            return AstVisitAction.Continue;
        }

        private void AnalyzeNewObjectCommand(CommandAst commandAst)
        {
            //new-object hashtable
            var typeNameAst = commandAst.CommandElements[1] as StringConstantExpressionAst;
            if (typeNameAst != null)
            {
                if (presetTypeNameSet.Contains(typeNameAst.Value))
                {
                    var dr = new DiagnosticRecord(
                        Strings.UseLiteralInitilializerForHashtableDescription,
                        commandAst.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        ruleId: null,
                        suggestedCorrections: null);
                    diagnosticRecords.Add(dr);
                }
            }
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

            // no arguments provided to new
            if (methodCallAst.Arguments == null)
            {

                var dr = new DiagnosticRecord(
                    Strings.UseLiteralInitilializerForHashtableDescription,
                    methodCallAst.Extent,
                    GetName(),
                    GetDiagnosticSeverity(),
                    fileName,
                    ruleId: null,
                    suggestedCorrections: null);
                diagnosticRecords.Add(dr);
            }

            return AstVisitAction.Continue;
        }

        public string GetCommonName()
        {
            return Strings.UseLiteralInitilializerForHashtableCommonName;
        }

        public string GetDescription()
        {
            return Strings.UseLiteralInitilializerForHashtableDescription;
        }

        public string GetName()
        {
            return Strings.UseLiteralInitilializerForHashtableName;
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
