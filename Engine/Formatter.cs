using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public class Formatter
    {
        // TODO add a method that takes range parameter
        public static string Format<TCmdlet>(
            string scriptDefinition,
            Settings settings,
            Range range,
            TCmdlet cmdlet) where TCmdlet : PSCmdlet, IOutputWriter
        {
            Helper.Instance = new Helper(cmdlet.SessionState.InvokeCommand, cmdlet);
            Helper.Instance.Initialize();

            var ruleOrder = new string[]
            {
                "PSPlaceCloseBrace",
                "PSPlaceOpenBrace",
                "PSUseConsistentWhitespace",
                "PSUseConsistentIndentation",
                "PSAlignAssignmentStatement"
            };

            var text = new EditableText(scriptDefinition);
            foreach (var rule in ruleOrder)
            {
                if (!settings.RuleArguments.ContainsKey(rule))
                {
                    continue;
                }

                cmdlet.WriteVerbose("Running " + rule);
                var currentSettings = GetCurrentSettings(settings, rule);
                ScriptAnalyzer.Instance.UpdateSettings(currentSettings);
                ScriptAnalyzer.Instance.Initialize(cmdlet, null, null, null, null, true, false);
                text = ScriptAnalyzer.Instance.Fix(text, range);
            }

            return text.ToString();
        }

        private static Settings GetCurrentSettings(Settings settings, string rule)
        {
            var currentSettingsHashtable = new Hashtable();
            currentSettingsHashtable.Add("IncludeRules", new string[] { rule });

            var ruleSettings = new Hashtable();
            ruleSettings.Add(rule, new Hashtable(settings.RuleArguments[rule]));
            currentSettingsHashtable.Add("Rules", ruleSettings);

            var currentSettings = new Settings(currentSettingsHashtable);
            return currentSettings;
        }
    }
}
