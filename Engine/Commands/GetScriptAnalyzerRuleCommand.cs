using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Resources;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// GetScriptAnalyzerRuleCommand: Cmdlet to list all the analyzer rule names and descriptions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ScriptAnalyzerRule", HelpUri = "http://go.microsoft.com/fwlink/?LinkId=525913")]
    public class GetScriptAnalyzerRuleCommand : PSCmdlet
    {
        #region Parameters
        /// <summary>
        /// Path: Path to custom rules folder.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] CustomizedRulePath
        {
            get { return customizedRulePath; }
            set { customizedRulePath = value; }
        }
        private string[] customizedRulePath;

        /// <summary>
        /// Name: The name of a specific rule to list.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Name
        {
            get { return name; }
            set { name = value; }
        }
        private string[] name;
        #endregion Parameters

        #region Private Members

        Dictionary<string, List<string>> validationResults = new Dictionary<string, List<string>>();

        #endregion

        #region Overrides

        /// <summary>
        /// BeginProcessing : TBD
        /// </summary>
        protected override void BeginProcessing()
        {
            #region Set PSCmdlet property of Helper

            Helper.Instance.MyCmdlet = this;

            #endregion
            // Verifies rule extensions
            if (customizedRulePath != null)
            {
                validationResults = ScriptAnalyzer.Instance.CheckRuleExtension(customizedRulePath, this);
                foreach (string extension in validationResults["InvalidPaths"])
                {
                    WriteWarning(string.Format(CultureInfo.CurrentCulture,Strings.MissingRuleExtension, extension));
                }
            }
            else
            {
                validationResults.Add("InvalidPaths", new List<string>());
                validationResults.Add("ValidModPaths", new List<string>());
                validationResults.Add("ValidDllPaths", new List<string>());            
            }

            try
            {
                if (validationResults["ValidDllPaths"].Count == 0)
                {
                    ScriptAnalyzer.Instance.Initialize();
                }
                else
                {
                    ScriptAnalyzer.Instance.Initilaize(validationResults);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, ex.HResult.ToString("X", CultureInfo.CurrentCulture), 
                    ErrorCategory.NotSpecified, this));
            }
        }

        /// <summary>
        /// ProcessRecord : TBD
        /// </summary>
        protected override void ProcessRecord()
        {
            string[] modNames = null;
            if (validationResults["ValidModPaths"].Count > 0)
            {
                modNames = validationResults["ValidModPaths"].ToArray<string>();
            }

            IEnumerable<IRule> rules = ScriptAnalyzer.Instance.GetRule(modNames, name);
            if (rules == null)
            {
                WriteObject(string.Format(CultureInfo.CurrentCulture, Strings.RulesNotFound));
            }
            else
            {
                foreach (IRule rule in rules)
                {
                    WriteObject(new RuleInfo(rule.GetName(), rule.GetCommonName(), rule.GetDescription(), rule.GetSourceType(), rule.GetSourceName()));
                }
            }
        }

        #endregion
    }
}
