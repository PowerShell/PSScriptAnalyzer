using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class AvoidGlobalAliases : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> records;
        private string fileName;

        /// <summary>
        /// Analyzes the ast to check that global aliases are not used.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            records = new List<DiagnosticRecord>();
            this.fileName = fileName;

            if (IsScriptModule())
            {
                ast.Visit(this);
            }

            return records;
        }

        #region VisitCommand functions
        /// <summary>
        /// Analyzes a CommandAst, if it is a New-Alias command, the AST is further analyzed.
        /// </summary>
        /// <param name="commandAst">The CommandAst to be analyzed</param>
        /// <returns>AstVisitAction to continue to analyze the ast's children</returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (!IsNewAliasCmdlet(commandAst))
            {
                return AstVisitAction.SkipChildren;
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Analyzes a CommandParameterAst for the global scope.
        /// </summary>
        /// <param name="commandParameterAst">The CommandParameterAst to be analyzed</param>
        /// <returns>AstVisitAction to skip child ast processing after creating any diagnostic records</returns>
        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            if (IsScopeParameterForNewAliasCmdlet(commandParameterAst))
            {
                // Check the commandParameterAst Argument property if it exist. This covers the case 
                // of the cmdlet looking like "New-Alias -Scope:Global"

                if ((commandParameterAst.Argument != null)
                    && (commandParameterAst.Argument.ToString().Equals("Global", StringComparison.OrdinalIgnoreCase)))
                {
                    records.Add(new DiagnosticRecord(
                                    string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesError),
                                    commandParameterAst.Extent,
                                    GetName(),
                                    DiagnosticSeverity.Warning,
                                    fileName,
                                    commandParameterAst.ParameterName));
                }
                else
                {
                    // If the commandParameterAst Argument property is null the next ast in the tree
                    // can still be a string const. This covers the case of the cmdlet looking like
                    // "New-Alias -Scope Global"

                    var nextAst = FindNextAst(commandParameterAst) as StringConstantExpressionAst;

                    if ((nextAst != null) 
                        && ((nextAst).Value.ToString().Equals("Global", StringComparison.OrdinalIgnoreCase)))
                    {
                        records.Add(new DiagnosticRecord(
                                string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesError),
                                (nextAst).Extent,
                                GetName(),
                                DiagnosticSeverity.Warning,
                                fileName,
                                (nextAst).Value));
                    }
                }
            }

            return AstVisitAction.SkipChildren;
        }
        #endregion

        /// <summary>
        /// Returns the next ast of the same level in the ast tree.
        /// </summary>
        /// <param name="ast">Ast used as a base</param>
        /// <returns>Next ast of the same level in the ast tree</returns>
        private Ast FindNextAst(Ast ast)
        {
            IEnumerable<Ast> matchingLevelAsts = ast.Parent.FindAll(item => item is Ast, true);

            Ast currentClosest = null;
            foreach (var matchingLevelAst in matchingLevelAsts)
            {
                if (currentClosest == null)
                {
                    if (IsAstAfter(ast, matchingLevelAst))
                    {
                        currentClosest = matchingLevelAst;
                    }
                }
                else
                {
                    if ((IsAstAfter(ast, matchingLevelAst)) && (IsAstAfter(matchingLevelAst, currentClosest)))
                    {
                        currentClosest = matchingLevelAst;
                    }
                }
            }

            return currentClosest;
        }

        /// <summary>
        /// Determines if ast1 is after ast2 in the ast tree.
        /// </summary>
        /// <param name="ast1">First ast</param>
        /// <param name="ast2">Second ast</param>
        /// <returns>True if ast2 is after ast1 in the ast tree</returns>
        private bool IsAstAfter(Ast ast1, Ast ast2)
        {
            if (ast1.Extent.EndLineNumber > ast2.Extent.StartLineNumber)  // ast1 ends on a line after ast2 starts
            {
                return false;
            }
            else if (ast1.Extent.EndLineNumber == ast2.Extent.StartLineNumber)
            {
                if (ast2.Extent.StartColumnNumber > ast1.Extent.EndColumnNumber)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else   // ast2 starts on a line after ast 1 ends
            {
                return true;
            }
        }

        /// <summary>
        /// Determines if CommandParameterAst is for the "Scope" parameter.
        /// </summary>
        /// <param name="commandParameterAst">CommandParameterAst to validate</param>
        /// <returns>True if the CommandParameterAst is for the Scope parameter</returns>
        private bool IsScopeParameterForNewAliasCmdlet(CommandParameterAst commandParameterAst)
        {
            if (commandParameterAst == null || commandParameterAst.ParameterName == null)
            {
                return false;
            }

            if (commandParameterAst.ParameterName.Equals("Scope", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if CommandAst is for the "New-Alias" command, checking aliases.
        /// </summary>
        /// <param name="commandAst">CommandAst to validate</param>
        /// <returns>True if the CommandAst is for the "New-Alias" command</returns>
        private bool IsNewAliasCmdlet(CommandAst commandAst)
        {
            if (commandAst == null || commandAst.GetCommandName() == null)
            {
                return false;
            }

            var AliasList = Helper.Instance.CmdletNameAndAliases("New-Alias");
            if (AliasList.Contains(commandAst.GetCommandName()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if analyzing a script module.
        /// </summary>
        /// <returns>True is file name ends with ".psm1"</returns>
        private bool IsScriptModule()
        {
            return fileName.EndsWith(".psm1");
        }

        public string GetCommonName()
        {
             return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesDescription);
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesName);
        }

        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
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
