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
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingPlainTextForPassword: Check that parameter "password", "passphrase" do not use plaintext
    /// (they should be of the type SecureString).
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUsingPlainTextForPassword : IScriptRule
    {
        /// <summary>
        /// AvoidUsingPlainTextForPassword: Check that parameter "password", "passphrase" and do not use plaintext.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all ParamAsts.
            IEnumerable<Ast> paramAsts = ast.FindAll(testAst => testAst is ParameterAst, true);

            List<String> passwords = new List<String>() {"Password", "Passphrase"};

            // Iterrates all ParamAsts and check if their names are on the list.
            foreach (ParameterAst paramAst in paramAsts)
            {
                TypeInfo paramType = (TypeInfo) paramAst.StaticType;
                bool hasPwd = false;
                String paramName = paramAst.Name.VariablePath.ToString();
                                
                foreach (String password in passwords)
                {
                    if (paramName.IndexOf(password, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        hasPwd = true;
                        break;
                    }
                }

                if (hasPwd && (!paramType.IsArray && paramType != typeof(System.Security.SecureString)
                              || (paramType.IsArray && paramType.GetElementType() != typeof(System.Security.SecureString))))
                {
                    yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPlainTextForPasswordError, paramAst.Name),
                        paramAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingPlainTextForPasswordName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPlainTextForPasswordCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPlainTextForPasswordDescription);
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
