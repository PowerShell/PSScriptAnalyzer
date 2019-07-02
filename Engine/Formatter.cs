// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// A class to provide code formatting capability.
    /// </summary>
    public class Formatter
    {
        private static readonly IEnumerable<string> s_formattingRulesInOrder = new []
        {
            "PSPlaceCloseBrace",
            "PSPlaceOpenBrace",
            "PSUseConsistentWhitespace",
            "PSUseConsistentIndentation",
            "PSAlignAssignmentStatement",
            "PSUseCorrectCasing"
        };

        /// <summary>
        /// Format a powershell script.
        /// </summary>
        /// <param name="scriptDefinition">A string representing a powershell script.</param>
        /// <param name="settings">Settings to be used for formatting</param>
        /// <param name="range">The range in which formatting should take place.</param>
        /// <param name="cmdlet">The cmdlet object that calls this method.</param>
        /// <returns></returns>
        public static string Format<TCmdlet>(
            string scriptDefinition,
            Settings settings,
            Range range,
            TCmdlet cmdlet) where TCmdlet : PSCmdlet, IOutputWriter
        {
            // todo implement notnull attribute for such a check
            ValidateNotNull(scriptDefinition, nameof(scriptDefinition));
            ValidateNotNull(settings, nameof(settings));
            ValidateNotNull(cmdlet, nameof(cmdlet));

            Helper.Instance = new Helper(cmdlet.SessionState.InvokeCommand, cmdlet);
            Helper.Instance.Initialize();

            Settings currentSettings = GetCurrentSettings(settings);
            ScriptAnalyzer.Instance.UpdateSettings(currentSettings);
            ScriptAnalyzer.Instance.Initialize(cmdlet, includeDefaultRules: true);

            try
            {
                return ScriptAnalyzer.Instance.Fix(scriptDefinition, range);
            }
            catch (FormattingException e)
            {
                cmdlet.ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "FIX_ERROR",
                        ErrorCategory.InvalidOperation,
                        e.Corrections));
                return null;
            }
        }

        private static void ValidateNotNull(object obj, string name)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        private static Settings GetCurrentSettings(Settings settings)
        {
            var ruleSettings = new Hashtable();
            foreach (string rule in s_formattingRulesInOrder)
            {
                if (settings.RuleArguments.TryGetValue(rule, out Dictionary<string, object> ruleConfiguration))
                {
                    ruleSettings[rule] = ruleConfiguration;
                }
            }

            return new Settings(new Hashtable()
            {
                { "IncludeRules", s_formattingRulesInOrder },
                { "Rules", ruleSettings }
            });
        }
    }

    public class FormattingException : Exception
    {
        public FormattingException(
            string message,
            IReadOnlyList<CorrectionExtent> corrections)
            : base(message)
        {
            Corrections = corrections;
        }

        public IReadOnlyList<CorrectionExtent> Corrections { get; }
    }
}
