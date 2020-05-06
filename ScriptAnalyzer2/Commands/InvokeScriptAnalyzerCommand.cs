using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using System;
using System.Collections.Concurrent;
using System.Management.Automation;

#if !CORECLR
using Microsoft.PowerShell.ScriptAnalyzer.Internal;
#endif

namespace Microsoft.PowerShell.ScriptAnalyzer.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "ScriptAnalyzer2")]
    public class InvokeScriptAnalyzerCommand : Cmdlet
    {
        private static readonly ConcurrentDictionary<ParameterSetting, ScriptAnalyzer> s_configuredScriptAnalyzers = new ConcurrentDictionary<ParameterSetting, ScriptAnalyzer>();

        private ScriptAnalyzer _scriptAnalyzer;

        [ValidateNotNullOrEmpty]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "FilePath")]
        public string[] Path { get; set; }

        [ValidateNotNullOrEmpty]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Input")]
        public string[] ScriptDefinition { get; set; }

        [Parameter]
        public string ConfigurationPath { get; set; }

        [Parameter]
        public string[] ExcludeRules { get; set; }

        protected override void BeginProcessing()
        {
            _scriptAnalyzer = GetScriptAnalyzer();
        }

        protected override void ProcessRecord()
        {
            if (Path != null)
            {
                foreach (string path in Path)
                {
                    foreach (ScriptDiagnostic diagnostic in _scriptAnalyzer.AnalyzeScriptPath(path))
                    {
                        WriteObject(diagnostic);
                    }
                }

                return;
            }

            if (ScriptDefinition != null)
            {
                foreach (string input in ScriptDefinition)
                {
                    foreach (ScriptDiagnostic diagnostic in _scriptAnalyzer.AnalyzeScriptInput(input))
                    {
                        WriteObject(diagnostic);
                    }
                }
            }
        }

        private ScriptAnalyzer GetScriptAnalyzer()
        {
            var parameters = new ParameterSetting(this);
            return s_configuredScriptAnalyzers.GetOrAdd(parameters, CreateScriptAnalyzerWithParameters);
        }

        private ScriptAnalyzer CreateScriptAnalyzerWithParameters(ParameterSetting parameters)
        {
            var configBuilder = new ScriptAnalyzerConfigurationBuilder()
                .WithBuiltinRuleSet(BuiltinRulePreference.Default);

            if (parameters.ConfigurationPath != null)
            {
                configBuilder.AddConfigurationFile(parameters.ConfigurationPath);
            }

            return configBuilder.Build().CreateScriptAnalyzer();
        }

        private struct ParameterSetting
        {
            public ParameterSetting(InvokeScriptAnalyzerCommand command)
            {
                ConfigurationPath = command.ConfigurationPath;
            }

            public string ConfigurationPath { get; }

            public override int GetHashCode()
            {
#if CORECLR
                return HashCode.Combine(ConfigurationPath);
#else
                return HashCodeCombinator.Create()
                    .Add(ConfigurationPath)
                    .GetHashCode();
#endif
            }
        }
    }
}
