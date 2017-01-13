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

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// The attribute class to designate if a property is configurable or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurableRulePropertyAttribute : Attribute
    {
        /// <summary>
        /// Default value of the property that the attribute decorates.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Initialize the attribute with the decorated property's default value.
        /// </summary>
        /// <param name="defaultValue"></param>
        public ConfigurableRulePropertyAttribute(object defaultValue)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException(nameof(defaultValue), Strings.ConfigurableScriptRuleNRE);
            }

            DefaultValue = defaultValue;
        }
    }
}
