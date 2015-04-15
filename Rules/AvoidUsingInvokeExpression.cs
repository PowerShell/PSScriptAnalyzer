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
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// InvokeExpressionRule: Check to make sure that Invoke-Expression is not used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class InvokeExpressionRule : AvoidCmdletGeneric
    {
        /// <summary>
        /// GetCmdletName: Retrieves the name of the cmdlet
        /// </summary>
        /// <returns></returns>
        public override string GetCmdletName()
        {
            return "invoke-expression";
        }

        /// <summary>
        /// GetError: Retrieves the error message
        /// </summary>
        /// <returns></returns>
        public override string GetError(String FileName)
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInvokeExpressionError);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingInvokeExpressionRuleName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInvokeExpressionRuleCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInvokeExpressionRuleDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

    }
}
