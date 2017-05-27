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
        private Settings settings;

        private Formatter(Settings settings)
        {
            this.settings = settings;
        }

        public static string Format(string scriptDefinition, Settings settings)
        {
            throw new NotImplementedException();
        }

        public static string Format(
            string scriptDefinition,
            Hashtable settingsHashtable,
            Runspace runspace,
            IOutputWriter outputWriter)
        {
            var inputSettings = new Settings(settingsHashtable);
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
                if (!inputSettings.RuleArguments.ContainsKey(rule))
                {
                    continue;
                }

                outputWriter.WriteVerbose("Running " + rule);
                var currentSettingsHashtable = new Hashtable();
                currentSettingsHashtable.Add("IncludeRules", new string[] { rule });
                var ruleSettings = new Hashtable();
                ruleSettings.Add(rule, new Hashtable(inputSettings.RuleArguments[rule]));
                currentSettingsHashtable.Add("Rules", ruleSettings);
                var currentSettings = new Settings(currentSettingsHashtable);
                ScriptAnalyzer.Instance.UpdateSettings(inputSettings);
                ScriptAnalyzer.Instance.Initialize(runspace, outputWriter);

                var corrections = new List<CorrectionExtent>();
                var records = Enumerable.Empty<DiagnosticRecord>();
                var numPreviousCorrections = corrections.Count;

                do
                {
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
                        outputWriter.ThrowTerminatingError(new ErrorRecord(
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
    }
}
