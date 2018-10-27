// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseLiteralInitializerForHashtable: Checks if hashtable is not initialized using [hashtable]::new or new-object hashtable.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseLiteralInitializerForHashtable : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> diagnosticRecords;
        private HashSet<string> presetTypeNameSet;
        private string fileName;

        public UseLiteralInitializerForHashtable()
        {
            var presetTypeNames = new string[]
            {
                "system.collections.hashtable",
                "collections.hashtable",
                "hashtable"
            };
            presetTypeNameSet = new HashSet<string>(presetTypeNames, StringComparer.OrdinalIgnoreCase);
            diagnosticRecords = new List<DiagnosticRecord>();
        }

        /// <summary>
        /// Analyzes the given ast to find if a hashtable is initialized using [hashtable]::new or New-Object Hashtable
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
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

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseLiteralInitilializerForHashtableCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseLiteralInitilializerForHashtableDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseLiteralInitilializerForHashtableName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// Visits command ast to check for "new-object" command
        /// </summary>
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

        /// <summary>
        /// Checks if a hashtable is created using [hashtable]::new()
        /// </summary>
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

        /// <summary>
        /// Analyzes command ast to check for new-object command and parse its arguments
        /// </summary>
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


            if (argumentList != null
                && HasIgnoreCaseComparerArg(argumentList))
            {
                return;
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

        /// <summary>
        /// Interpret the named and unnamed arguments and assign them their corresponding parameters
        ///
        /// PSv4 onwards there exists System.Management.Automation.Language.StaticParameterBinder.BindCommand to
        /// achieve identical objective. But since we support PSSA on PSv3 too we need this implementation.
        /// </summary>
        /// <param name="commandAst">An non-null instance of CommandAst. Expects it be commandast of "new-object" command</param>
        /// <param name="typeName">Returns the TypeName argument</param>
        /// <param name="argumentList">Returns the ArgumentList argument</param>
        /// This should read the command in all the following form
        /// new-object hashtable
        /// new-object -Typename hashtable
        /// new-object hashtable -ArgumentList comparer
        /// new-object -Typename hashtable -ArgumentList blah1,blah2
        /// new-object -ArgumentList blah1,blah2 -typename hashtable
        private void GetParametersFromCommandAst(CommandAst commandAst, out string typeName, out List<string> argumentList)
        {
            argumentList = null;
            typeName = null;
            var namedArguments = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            namedArguments.Add("typename", null);
            namedArguments.Add("argumentlist", null);
            var positinalElems = GetNamedArguments(commandAst.CommandElements, ref namedArguments);
            GetPositionalArguments(new ReadOnlyCollection<CommandElementAst>(positinalElems), ref namedArguments);
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

        /// <summary>
        /// Returns the first index whose value is null
        /// </summary>
        /// <param name="namedArguments">An ordered dictionary. It must not be null</param>
        /// <returns>Returns the first index whose value is null else returns -1</returns>
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

        /// <summary>
        /// Assigns the unnamed arguments to their corresponding parameters
        /// </summary>
        /// <param name="unnamedArguments">A collection of arguments that need to be assigned</param>
        /// <param name="namedArguments">An ordered dictionary of parameters in their corresponding positions</param>
        private void GetPositionalArguments(ReadOnlyCollection<CommandElementAst> unnamedArguments, ref OrderedDictionary namedArguments)
        {
            for (int k = 0; k < unnamedArguments.Count; k++)
            {
                int firstEmptyIndex = GetFirstEmptyIndex(namedArguments);
                if (firstEmptyIndex == -1)
                {
                    return;
                }

                var elem = unnamedArguments[k];
                namedArguments[firstEmptyIndex] = elem as ExpressionAst;
            }
        }

        /// <summary>
        /// Gets the named arguments from a list of command elements
        /// </summary>
        /// <param name="commandElements">A list of command elements, typically a property of CommandAst instance</param>
        /// <param name="namedArguments">An ordered dictionary of parameters in their corresponding positions</param>
        /// <returns>Returns a list of unnamed arguments that remain after taking into account named parameters</returns>
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

        /// <summary>
        /// Checks if any argument in the given collection ends with "ignorecase"
        /// </summary>
        /// <param name="arguments">A collection of argument asts. Neither this nor any elements within it should be null</param>
        private bool HasIgnoreCaseComparerArg(ReadOnlyCollection<ExpressionAst> arguments)
        {
            var argumentsAsStrings = new List<string>();
            foreach (var arg in arguments)
            {
                var memberExprAst = arg as MemberExpressionAst;
                argumentsAsStrings.Add(null);
                if (memberExprAst == null)
                {
                    continue;
                }

                var strConstExprAst = memberExprAst.Member as StringConstantExpressionAst;
                if (strConstExprAst == null)
                {
                    continue;
                }
                argumentsAsStrings[argumentsAsStrings.Count - 1] = strConstExprAst.Value;
            }
            return HasIgnoreCaseComparerArg(argumentsAsStrings);
        }

        /// <summary>
        /// Checks if any argument in the given collection ends with "ignorecase"
        /// </summary>
        /// <param name="arguments">An enumerable of type string. Elements can be null but the collection must be non-null  .</param>
        /// <returns></returns>
        private bool HasIgnoreCaseComparerArg(IEnumerable<string> arguments)
        {

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    continue;
                }
                if (arg.EndsWith("ignorecase", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Suggested corrections to replace the violations
        /// </summary>
        /// <param name="violation">Ast representing the violation. This should not be null.</param>
        /// <param name="filename">Name of file containing the violation</param>
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
    }
}
