// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseDeclaredVarsMoreThanAssignments: Analyzes the ast to check that variables are used in more than just their assignment.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseDeclaredVarsMoreThanAssignments : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that variables are used in more than just there assignment.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of results from this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            var scriptBlockAsts = ast.FindAll(x => x is ScriptBlockAst, true);
            if (scriptBlockAsts == null)
            {
                yield break;
            }

            foreach (var scriptBlockAst in scriptBlockAsts)
            {
                var sbAst = scriptBlockAst as ScriptBlockAst;
                foreach (var diagnosticRecord in AnalyzeScriptBlockAst(sbAst, fileName))
                {
                    yield return diagnosticRecord;
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseDeclaredVarsMoreThanAssignmentsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseDeclaredVarsMoreThanAssignmentsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseDeclaredVarsMoreThanAssignmentsDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Checks if a variable is initialized and referenced in either its assignment or children scopes
        /// </summary>
        /// <param name="scriptBlockAst">Ast of type ScriptBlock</param>
        /// <param name="fileName">Name of file containing the ast</param>
        /// <returns>An enumerable containing diagnostic records</returns>
        private IEnumerable<DiagnosticRecord> AnalyzeScriptBlockAst(ScriptBlockAst scriptBlockAst, string fileName)
        {
            IEnumerable<Ast> assignmentAsts = scriptBlockAst.FindAll(testAst => testAst is AssignmentStatementAst, false);
            IEnumerable<Ast> varAsts = scriptBlockAst.FindAll(testAst => testAst is VariableExpressionAst, true);
            IEnumerable<Ast> varsInAssignment;

            Dictionary<string, AssignmentStatementAst> assignments = new Dictionary<string, AssignmentStatementAst>(StringComparer.OrdinalIgnoreCase);

            string varKey;
            bool inAssignment;

            if (assignmentAsts == null)
            {
                yield break;
            }

            foreach (AssignmentStatementAst assignmentAst in assignmentAsts)
            {
                // Only checks for the case where lhs is a variable. Ignore things like $foo.property
                VariableExpressionAst assignmentVarAst = assignmentAst.Left as VariableExpressionAst;

                if (assignmentVarAst == null)
                {
                    // If the variable is declared in a strongly typed way, e.g. [string]$s = 'foo' then the type is ConvertExpressionAst.
                    // Therefore we need to the VariableExpressionAst from its Child property.
                    var assignmentVarAstAsConvertExpressionAst = assignmentAst.Left as ConvertExpressionAst;
                    if (assignmentVarAstAsConvertExpressionAst != null && assignmentVarAstAsConvertExpressionAst.Child != null)
                    {
                        assignmentVarAst = assignmentVarAstAsConvertExpressionAst.Child as VariableExpressionAst;
                    }
                }

                if (assignmentVarAst != null)
                {
                    // Ignore if variable is global or environment variable or scope is function
                    if (!Helper.Instance.IsVariableGlobalOrEnvironment(assignmentVarAst, scriptBlockAst)
                        && !assignmentVarAst.VariablePath.IsScript
                        && !string.Equals(assignmentVarAst.VariablePath.DriveName, "function", StringComparison.OrdinalIgnoreCase))
                    {
                        string variableName = Helper.Instance.VariableNameWithoutScope(assignmentVarAst.VariablePath);

                        if (!assignments.ContainsKey(variableName))
                        {
                            assignments.Add(variableName, assignmentAst);
                        }
                    }
                }
            }

            if (varAsts != null)
            {
                foreach (VariableExpressionAst varAst in varAsts)
                {
                    varKey = Helper.Instance.VariableNameWithoutScope(varAst.VariablePath);
                    inAssignment = false;

                    if (assignments.ContainsKey(varKey))
                    {
                        varsInAssignment = assignments[varKey].Left.FindAll(testAst => testAst is VariableExpressionAst, true);

                        // Checks if this variableAst is part of the logged assignment
                        foreach (VariableExpressionAst varInAssignment in varsInAssignment)
                        {
                            // Try casting to AssignmentStatementAst to be able to catch case where a variable is assigned more than once (https://github.com/PowerShell/PSScriptAnalyzer/issues/833)
                            var varInAssignmentAsStatementAst = varInAssignment.Parent as AssignmentStatementAst;
                            var varAstAsAssignmentStatementAst = varAst.Parent as AssignmentStatementAst;
                            if (varInAssignmentAsStatementAst != null && varAstAsAssignmentStatementAst != null)
                            {
                                inAssignment = varInAssignmentAsStatementAst.Left.Extent.Text.Equals(varAstAsAssignmentStatementAst.Left.Extent.Text, StringComparison.OrdinalIgnoreCase);
                            }
                            else
                            {
                                inAssignment = varInAssignment.Equals(varAst);
                            }
                        }

                        if (!inAssignment)
                        {
                            assignments.Remove(varKey);
                        }

                        // Check if variable belongs to PowerShell built-in variables
                        if (Helper.Instance.HasSpecialVars(varKey))
                        {
                            assignments.Remove(varKey);
                        }
                    }
                }
            }

            // Detect usages of Get-Variable
            var getVariableCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Get-Variable");
            IEnumerable<Ast> getVariableCommandAsts = scriptBlockAst.FindAll(testAst => testAst is CommandAst commandAst &&
                getVariableCmdletNamesAndAliases.Contains(commandAst.GetCommandName(), StringComparer.OrdinalIgnoreCase), true);
            foreach (CommandAst getVariableCommandAst in getVariableCommandAsts)
            {
                var commandElements = getVariableCommandAst.CommandElements.ToList();
                // The following extracts the variable name only in the simplest possibe case 'Get-Variable variableName'
                if (commandElements.Count == 2 && commandElements[1] is StringConstantExpressionAst constantExpressionAst)
                {
                    var variableName = constantExpressionAst.Value;
                    if (assignments.ContainsKey(variableName))
                    {
                        assignments.Remove(variableName);
                    }
                }
            }

            foreach (string key in assignments.Keys)
            {
                yield return new DiagnosticRecord(
                    string.Format(CultureInfo.CurrentCulture, Strings.UseDeclaredVarsMoreThanAssignmentsError, key),
                    assignments[key].Left.Extent,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    fileName,
                    key);
            }
        }
    }
}
