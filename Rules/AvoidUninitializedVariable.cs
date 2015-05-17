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
    /// AvoidUnitializedVariable: Check if any uninitialized variable is used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUnitializedVariable : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if any uninitialized variable is used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            // TODO : We need to find another way for using S.M.A.L.Compiler.GetExpressionValue.
            // Following code are not working for certain scenarios;, like:
            // for ($i=1; $i -le 10; $i++){Write-Host $i}

            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all VariableExpressionAst
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is VariableExpressionAst, true);

            // Iterates all VariableExpressionAst and check the command name.
            foreach (VariableExpressionAst varAst in foundAsts)
            {
                if (Helper.Instance.IsUninitialized(varAst, ast))
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableError, varAst.VariablePath.UserPath),
                        varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName, varAst.VariablePath.UserPath);
                }
            }

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);
            IEnumerable<Ast> funcMemberAsts = ast.FindAll(item => item is FunctionMemberAst, true);
            
            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                // Finds all VariableExpressionAst.
                IEnumerable<Ast> varAsts = funcAst.FindAll(testAst => testAst is VariableExpressionAst, true);

                HashSet<string> paramVariables = new HashSet<string>();

                // don't raise the rules for variables in the param block.
                if (funcAst.Body != null && funcAst.Body.ParamBlock != null && funcAst.Body.ParamBlock.Parameters != null)
                {
                    paramVariables.UnionWith(funcAst.Body.ParamBlock.Parameters.Select(paramAst => paramAst.Name.VariablePath.UserPath));
                }
                
                //don't raise the rules for parameters outside the param block
                if(funcAst.Parameters != null)
                {
                    paramVariables.UnionWith(funcAst.Parameters.Select(paramAst => paramAst.Name.VariablePath.UserPath));
                }

                // Iterates all VariableExpressionAst and check the command name.
                foreach (VariableExpressionAst varAst in varAsts)
                {
                    if (Helper.Instance.IsUninitialized(varAst, funcAst) && !paramVariables.Contains(varAst.VariablePath.UserPath))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableError, varAst.VariablePath.UserPath),
                            varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName, varAst.VariablePath.UserPath);
                    }
                }
            }

            foreach (FunctionMemberAst funcMemAst in funcMemberAsts)
            {
                // Finds all VariableExpressionAst.
                IEnumerable<Ast> varAsts = funcMemAst.FindAll(testAst => testAst is VariableExpressionAst, true);

                // Iterates all VariableExpressionAst and check the command name.
                foreach (VariableExpressionAst varAst in varAsts)
                {
                    if (Helper.Instance.IsUninitialized(varAst, funcMemAst))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableError, varAst.VariablePath.UserPath),
                            varAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName, varAst.VariablePath.UserPath);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUninitializedVariableName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUninitializedVariableDescription);
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
