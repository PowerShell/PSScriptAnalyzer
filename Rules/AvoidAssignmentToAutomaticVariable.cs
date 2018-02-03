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
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidAssignmentToAutomaticVariable: 
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidAssignmentToAutomaticVariable : IScriptRule
    {
        private static readonly IList<string> _readOnlyAutomaticVariables = new List<string>()
            {
                // Attempting to assign to any of those read-only variable would result in an error at runtime
                "?", "true", "false", "Host", "PSCulture", "Error", "ExecutionContext", "Home", "PID", "PSEdition", "PSHome", "PSUICulture", "PSVersionTable", "SHellId"
            };

        /// <summary>
        /// Checks for assignment to automatic variables.
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>The diagnostic results of this rule</returns>
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);


            IEnumerable<Ast> assignmentStatementAsts = ast.FindAll(testAst => testAst is AssignmentStatementAst, searchNestedScriptBlocks: true);
            IEnumerable<Ast> parameterAsts = ast.FindAll(testAst => testAst is ParameterAst, searchNestedScriptBlocks: true);
            

            var assignmentStatementAndParamterAsts = assignmentStatementAsts.Concat(parameterAsts);
            foreach (var assignmentStatementAst in assignmentStatementAndParamterAsts)
            {
                var variableExpressionAst = assignmentStatementAst.Find(testAst => testAst is VariableExpressionAst, searchNestedScriptBlocks: false) as VariableExpressionAst;
                var variableName = variableExpressionAst.VariablePath.UserPath;
                if (_readOnlyAutomaticVariables.Contains(variableName, StringComparer.OrdinalIgnoreCase))
                {
                    yield return new DiagnosticRecord(DiagnosticRecordHelper.FormatError(Strings.AvoidAssignmentToReadOnlyAutomaticVariableError, variableName),
                                                      variableExpressionAst.Extent, GetName(), DiagnosticSeverity.Error, fileName);
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidAssignmentToAutomaticVariableName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidAssignmentToReadOnlyAutomaticVariableCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidAssignmentToReadOnlyAutomaticVariableDescription);
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
    }

}
