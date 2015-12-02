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
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsernameAndPasswordParams: Check that a function does not use both username and password
    /// parameters.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUsernameAndPasswordParams : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check that a function does not use both username
        /// and password parameters.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all functionAst
            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            List<String> passwords = new List<String>() {"Password", "Passphrase"};
            List<String> usernames = new List<String>() { "Username", "User"};

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                bool hasPwd = false;
                bool hasUserName = false;

                // Finds all ParamAsts.
                IEnumerable<Ast> paramAsts = funcAst.FindAll(testAst => testAst is ParameterAst, true);
                // Iterrates all ParamAsts and check if their names are on the list.
                foreach (ParameterAst paramAst in paramAsts)
                {
                    TypeInfo paramType = (TypeInfo)paramAst.StaticType;
                    String paramName = paramAst.Name.VariablePath.ToString();

                    // if this is pscredential type, skip
                    if (paramType == typeof(PSCredential) || (paramType.IsArray && paramType.GetElementType() == typeof (PSCredential)))
                    {
                        continue;
                    }

                    // if this has credential attribute, skip
                    if (paramAst.Attributes.Any(paramAttribute => paramAttribute.TypeName.GetReflectionType() == typeof(CredentialAttribute)))
                    {
                        continue;
                    }

                    foreach (String password in passwords)
                    {
                        if (paramName.IndexOf(password, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            hasPwd = true;
                            break;
                        }
                    }

                    foreach (String username in usernames)
                    {
                        if (paramName.IndexOf(username, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            hasUserName = true;
                            break;
                        }
                    }
                }

                if (hasUserName && hasPwd)
                {
                    yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsernameAndPasswordParamsError, funcAst.Name),
                        funcAst.Extent, GetName(), DiagnosticSeverity.Error, fileName);
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsernameAndPasswordParamsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsernameAndPasswordParamsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsernameAndPasswordParamsDescription);
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
            return RuleSeverity.Error;
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
