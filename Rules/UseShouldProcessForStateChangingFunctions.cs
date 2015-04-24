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
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{/// <summary>
    /// UseShouldProcessCorrectly: Analyzes the ast to check that if the ShouldProcess attribute is present, the function calls ShouldProcess and vice versa.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UseShouldProcessForStateChangingFunctions : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check if ShouldProcess is included in Advanced functions if the Verb of the function could change system state.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> funcDefAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            string supportsShouldProcess = "SupportsShouldProcess";
            string trueString = "$true";
            foreach (FunctionDefinitionAst funcDefAst in funcDefAsts)
            {
                string funcName = funcDefAst.Name;
                bool hasShouldProcessAttribute = false;

                if (funcName.StartsWith("Restart-", StringComparison.OrdinalIgnoreCase) ||
                    funcName.StartsWith("Stop-", StringComparison.OrdinalIgnoreCase)||
                    funcName.StartsWith("New-", StringComparison.OrdinalIgnoreCase) ||
                    funcName.StartsWith("Set-", StringComparison.OrdinalIgnoreCase) ||
                    funcName.StartsWith("Update-", StringComparison.OrdinalIgnoreCase) ||
                    funcName.StartsWith("Reset-", StringComparison.OrdinalIgnoreCase))
                {
                    IEnumerable<Ast> attributeAsts = funcDefAst.FindAll(testAst => testAst is NamedAttributeArgumentAst, true);
                    if (funcDefAst.Body != null && funcDefAst.Body.ParamBlock != null
                        && funcDefAst.Body.ParamBlock.Attributes != null &&
                        funcDefAst.Body.ParamBlock.Parameters != null)
                    {
                        if (!funcDefAst.Body.ParamBlock.Attributes.Any(attr => attr.TypeName.GetReflectionType() == typeof (CmdletBindingAttribute)))
                        {
                            continue;
                        }

                        foreach (NamedAttributeArgumentAst attributeAst in attributeAsts)
                        {
                            if (attributeAst.ArgumentName.Equals(supportsShouldProcess, StringComparison.OrdinalIgnoreCase))
                            {
                                if((attributeAst.Argument.Extent.Text.Equals(trueString, StringComparison.OrdinalIgnoreCase)) && !attributeAst.ExpressionOmitted || 
                                    attributeAst.ExpressionOmitted)
                                {
                                    hasShouldProcessAttribute = true;
                                }
                            }
                        }

                        if (!hasShouldProcessAttribute)
                        {
                            yield return
                                 new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture,Strings.UseShouldProcessForStateChangingFunctionsError, funcName),funcDefAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseShouldProcessForStateChangingFunctionsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseShouldProcessForStateChangingFunctionsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseShouldProcessForStateChangingFunctionsDescrption);
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
