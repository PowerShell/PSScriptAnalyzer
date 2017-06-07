using System.Collections;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public class Formatter
    {
        public static string Format<TCmdlet>(
            string scriptDefinition,
            Settings settings,
            Range range,
            TCmdlet cmdlet) where TCmdlet : PSCmdlet, IOutputWriter
        {
            // todo add argument check
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

                Range updatedRange;
                text = ScriptAnalyzer.Instance.Fix(text, range, out updatedRange);
                range = updatedRange;
            }

            return text.ToString();
        }

        private static Settings GetCurrentSettings(Settings settings, string rule)
        {
            return new Settings(new Hashtable()
            {
                {"IncludeRules", new string[] {rule}},
                {"Rules", new Hashtable() { { rule, new Hashtable(settings.RuleArguments[rule]) } } }
            });
        }
    }
}
