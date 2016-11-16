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
// TODO Code formatting
        private Dictionary<ScriptBlockAst, Dictionary<string, AssignmentStatementAst>> scriptBlockAssignmentMap;
        private Dictionary<ScriptBlockAst, Dictionary<string, bool>> scriptblockVariableUsageMap;
        private Dictionary<ScriptBlockAst, ScriptBlockAst> scriptBlockAstParentMap;
        private Ast ast;
        private string fileName;

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

            scriptBlockAssignmentMap = new Dictionary<ScriptBlockAst, Dictionary<string, AssignmentStatementAst>>();
            scriptblockVariableUsageMap = new Dictionary<ScriptBlockAst, Dictionary<string, bool>>();
            scriptBlockAstParentMap = new Dictionary<ScriptBlockAst, ScriptBlockAst>();
            this.ast = ast;
            this.fileName = fileName;
            foreach (var scriptBlockAst in scriptBlockAsts)
            {
                var sbAst = scriptBlockAst as ScriptBlockAst;
                AnalyzeScriptBlockAst(sbAst, fileName);
            }

            foreach (var diagnosticRecord in GetViolations())
            {
                yield return diagnosticRecord;
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
        private void AnalyzeScriptBlockAst(ScriptBlockAst scriptBlockAst, string fileName)
        {
            IEnumerable<Ast> assignmentAsts = scriptBlockAst.FindAll(testAst => testAst is AssignmentStatementAst, false);
            IEnumerable<Ast> varAsts = scriptBlockAst.FindAll(testAst => testAst is VariableExpressionAst, true);
            Dictionary<string, AssignmentStatementAst> assignments = new Dictionary<string, AssignmentStatementAst>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, bool> isVariableUsed = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string varKey;
            bool inAssignment;

            if (assignmentAsts == null)
            {
                return;
            }

            foreach (AssignmentStatementAst assignmentAst in assignmentAsts)
            {
                // Only checks for the case where lhs is a variable. Ignore things like $foo.property
                VariableExpressionAst assignmentVarAst = assignmentAst.Left as VariableExpressionAst;

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
                            isVariableUsed.Add(variableName, false);
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
                        // Check if varAst is part of an AssignmentStatementAst
                        inAssignment = varAst.Parent is AssignmentStatementAst;

                        // Check if variable belongs to PowerShell built-in variables
                        // Checks if this variableAst is part of the logged assignment
                        if (!inAssignment || Helper.Instance.HasSpecialVars(varKey))
                        {
                            isVariableUsed[varKey] = true;
                        }
                    }
                }
            }

            scriptBlockAssignmentMap[scriptBlockAst] = assignments;
            scriptblockVariableUsageMap[scriptBlockAst] = isVariableUsed;
            scriptBlockAstParentMap[scriptBlockAst] = GetScriptBlockAstParent(scriptBlockAst);
        }

        /// <summary>
        /// Gets the first upstream node away from the input argument that is of type ScriptBlockAst
        /// </summary>
        /// <param name="scriptBlockAst">Ast</param>
        /// <returns>Null if the input argument's Parent is null
        /// or if Parent is ast, the input to AnalyzeAst</returns>
        private ScriptBlockAst GetScriptBlockAstParent(Ast scriptBlockAst)
        {
            if (scriptBlockAst == this.ast
                || scriptBlockAst.Parent == null)
            {
                return null;
            }

            var parent = scriptBlockAst.Parent as ScriptBlockAst;
            if (parent == null)
            {
                return GetScriptBlockAstParent(scriptBlockAst.Parent);
            }

            return parent;
        }

        /// <summary>
        /// Returns the violations in the given ast
        /// </summary>
        private IEnumerable<DiagnosticRecord> GetViolations()
        {
            foreach (var sbAst in scriptblockVariableUsageMap.Keys)
            {
                foreach (var variable in scriptblockVariableUsageMap[sbAst].Keys)
                {
                    if (!DoesScriptBlockUseVariable(sbAst, variable))
                    {
                        yield return new DiagnosticRecord(
                            string.Format(CultureInfo.CurrentCulture, Strings.UseDeclaredVarsMoreThanAssignmentsError, variable),
                            scriptBlockAssignmentMap[sbAst][variable].Left.Extent,
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName,
                            variable);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the input variable argument is used more than just declaration in
        /// the input scriptblock or in its parent scriptblock, otherwise returns false
        /// </summary>
        private bool DoesScriptBlockUseVariable(ScriptBlockAst scriptBlockAst, string variable)
        {
            if (scriptblockVariableUsageMap[scriptBlockAst].ContainsKey(variable))
            {
                if (!scriptblockVariableUsageMap[scriptBlockAst][variable])
                {
                    if (scriptBlockAstParentMap[scriptBlockAst] == null)
                    {
                        return false;
                    }

                    return DoesScriptBlockUseVariable(scriptBlockAstParentMap[scriptBlockAst], variable);
                }

                return true;
            }

            return false;
        }

    }
}
