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

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    internal class ExternalRule : IExternalRule
    {
        #region Methods

        string name    = string.Empty;
        string commonName = string.Empty;
        string desc    = string.Empty;
        string param   = string.Empty;
        string srcName = string.Empty;
        string modPath = string.Empty;
        string paramType = string.Empty;

        public string GetName()
        {
            return this.name;
        }

        public string GetCommonName()
        {
            return this.commonName;
        }

        public string GetDescription()
        {
            return this.desc;
        }

        public string GetParameter()
        {
            return this.param;
        }

        public SourceType GetSourceType()
        {
            return SourceType.Module;
        }

        public string GetParameterType()
        {
            return this.paramType;
        }

        //Set the community rule level as warning as the current implementation does not require user to specify rule severity when defining their functions in PS scripts
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public string GetSourceName()
        {
            return this.srcName;
        }

        public string GetFullModulePath()
        {
            return this.modPath;
        }

        #endregion

        #region Constructors
        
        public ExternalRule()
        {

        }

        public ExternalRule(string name, string commonName, string desc, string param, string paramType, string srcName, string modPath)
        {
            this.name    = name;
            this.commonName = commonName;
            this.desc    = desc;
            this.param   = param;
            this.srcName = srcName;
            this.modPath = modPath;
            this.paramType = paramType;
        }

        #endregion
    }
}
