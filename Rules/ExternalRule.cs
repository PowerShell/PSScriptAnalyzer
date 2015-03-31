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

        public ExternalRule(string name, string commonName, string desc, string param, string srcName, string modPath)
        {
            this.name    = name;
            this.commonName = commonName;
            this.desc    = desc;
            this.param   = param;
            this.srcName = srcName;
            this.modPath = modPath;
        }

        #endregion
    }
}
