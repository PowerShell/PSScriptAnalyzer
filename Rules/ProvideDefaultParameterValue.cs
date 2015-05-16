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
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ProvideDefaultParameterValue: Check if any uninitialized variable is used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class ProvideDefaultParameterValue : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if any uninitialized variable is used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all functionAst
            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                // Finds all ParamAsts.
                IEnumerable<Ast> varAsts = funcAst.FindAll(testAst => testAst is VariableExpressionAst, true);

                // Iterrates all ParamAsts and check if their names are on the list.

                HashSet<string> paramVariables = new HashSet<string>();
                // only raise the rules for variables in the param block.
                if (funcAst.Body != null && funcAst.Body.ParamBlock != null && funcAst.Body.ParamBlock.Parameters != null)
                {
                    foreach (var paramAst in funcAst.Body.ParamBlock.Parameters)
                    {
                        if (Helper.Instance.IsUninitialized(paramAst.Name, funcAst))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ProvideDefaultParameterValueError, paramAst.Name.VariablePath.UserPath),
                            paramAst.Name.Extent, GetName(), DiagnosticSeverity.Warning, fileName, paramAst.Name.VariablePath.UserPath);
                        }
                    }
                }

                if (funcAst.Parameters != null)
                {
                    foreach (var paramAst in funcAst.Parameters)
                    {
                        if (Helper.Instance.IsUninitialized(paramAst.Name, funcAst))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ProvideDefaultParameterValueError, paramAst.Name.VariablePath.UserPath),
                            paramAst.Name.Extent, GetName(), DiagnosticSeverity.Warning, fileName, paramAst.Name.VariablePath.UserPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ProvideDefaultParameterValueName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideDefaultParameterValueCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideDefaultParameterValueDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
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
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
