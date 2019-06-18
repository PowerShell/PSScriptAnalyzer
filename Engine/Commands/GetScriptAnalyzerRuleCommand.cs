// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// GetScriptAnalyzerRuleCommand: Cmdlet to list all the analyzer rule names and descriptions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ScriptAnalyzerRule", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=525913")]
    [OutputType(typeof(RuleInfo))]
    public class GetScriptAnalyzerRuleCommand : PSCmdlet, IOutputWriter
    {
        #region Parameters
        /// <summary>
        /// Path: Path to custom rules folder.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [Alias("CustomizedRulePath")]
        public string[] CustomRulePath
        {
            get { return customRulePath; }
            set { customRulePath = value; }
        }
        private string[] customRulePath;

        /// <summary>
        /// RecurseCustomRulePath: Find rules within subfolders under the path
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter RecurseCustomRulePath
        {
            get { return recurseCustomRulePath; }
            set { recurseCustomRulePath = value; }
        }
        private bool recurseCustomRulePath;

        /// <summary>
        /// Name: The name of a specific rule to list.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Name
        {
            get { return name; }
            set { name = value; }
        }
        private string[] name;

        /// <summary>
        /// Severity: Array of the severity types to be enabled.
        /// </summary>
        /// </summary>
        [ValidateSet("Warning", "Error", "Information", IgnoreCase = true)]
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Severity
        {
            get { return severity; }
            set { severity = value; }
        }
        private string[] severity;

        #endregion Parameters

        #region Overrides

        /// <summary>
        /// BeginProcessing : TBD
        /// </summary>
        protected override void BeginProcessing()
        {

            // Initialize helper
            Helper.Instance = new Helper(
                SessionState.InvokeCommand,
                this);
            Helper.Instance.Initialize();

            string[] rulePaths = Helper.ProcessCustomRulePaths(customRulePath,
                this.SessionState, recurseCustomRulePath);
            ScriptAnalyzer.Instance.Initialize(this, rulePaths, null, null, null, null == rulePaths ? true : false);
        }

        /// <summary>
        /// ProcessRecord : TBD
        /// </summary>
        protected override void ProcessRecord()
        {
            string[] modNames = ScriptAnalyzer.Instance.GetValidModulePaths();

            IEnumerable<IRule> rules = ScriptAnalyzer.Instance.GetRule(modNames, name);
            if (rules == null)
            {
                WriteObject(string.Format(CultureInfo.CurrentCulture, Strings.RulesNotFound));
            }
            else
            {
                if (severity != null)
                {
                    var ruleSeverity = severity.Select(item => Enum.Parse(typeof (RuleSeverity), item, true));
                    rules = rules.Where(item => ruleSeverity.Contains(item.GetSeverity())).ToList();
                }

                foreach (IRule rule in rules)
                {
                    WriteObject(new RuleInfo(rule.GetName(), rule.GetCommonName(), rule.GetDescription(),
                        rule.GetSourceType(), rule.GetSourceName(), rule.GetSeverity(), rule.GetType()));
                }
            }
        }

        #endregion
    }
}
