// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
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

        public string GetFullName()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}\\{1}", this.GetSourceName(), this.name);
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
