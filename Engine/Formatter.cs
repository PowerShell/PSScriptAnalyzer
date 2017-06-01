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

                var corrections = new List<CorrectionExtent>();
                var records = Enumerable.Empty<DiagnosticRecord>();
                var numPreviousCorrections = corrections.Count;

                do
                {
                    // TODO create better verbose messages
                    var correctionApplied = new HashSet<int>();
                    foreach (var correction in corrections)
                    {
                        // apply only one edit per line
                        if (correctionApplied.Contains(correction.StartLineNumber))
                        {
                            continue;
                        }

                        correctionApplied.Add(correction.StartLineNumber);
                        text.ApplyEdit(correction);
                    }

                    records = ScriptAnalyzer.Instance.AnalyzeScriptDefinition(text.ToString());
                    corrections = records.Select(r => r.SuggestedCorrections.ElementAt(0)).ToList();
                    if (numPreviousCorrections > 0 && numPreviousCorrections == corrections.Count)
                    {
                        cmdlet.ThrowTerminatingError(new ErrorRecord(
                            new InvalidOperationException(),
                            "FORMATTER_ERROR",
                            ErrorCategory.InvalidOperation,
                            corrections));
                    }

                    numPreviousCorrections = corrections.Count;

                    // get unique correction instances
                    // sort them by line numbers
                    corrections.Sort((x, y) =>
                    {
                        return x.StartLineNumber < x.StartLineNumber ?
                                    1 :
                                    (x.StartLineNumber == x.StartLineNumber ? 0 : -1);
                    });

                } while (numPreviousCorrections > 0);
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
