using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration.Json
{
    public class JsonScriptAnalyzerConfiguration : IScriptAnalyzerConfiguration
    {
        private static readonly JsonConfigurationConverter s_jsonConfigurationConverter = new JsonConfigurationConverter();

        public static JsonScriptAnalyzerConfiguration FromString(string jsonString)
        {
            return JsonConvert.DeserializeObject<JsonScriptAnalyzerConfiguration>(jsonString, s_jsonConfigurationConverter);
        }

        public static JsonScriptAnalyzerConfiguration FromFile(string filePath)
        {
            var serializer = new JsonSerializer()
            {
                Converters = { s_jsonConfigurationConverter },
            };

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<JsonScriptAnalyzerConfiguration>(jsonReader);
            }
        }

        private readonly IReadOnlyDictionary<string, JsonRuleConfiguration> _ruleConfigurations;

        public JsonScriptAnalyzerConfiguration(
            BuiltinRulePreference? builtinRulePreference,
            RuleExecutionMode? ruleExecutionMode,
            IReadOnlyList<string> rulePaths,
            IReadOnlyDictionary<string, JsonRuleConfiguration> ruleConfigurations)
        {
            _ruleConfigurations = ruleConfigurations;
            RulePaths = rulePaths;
            RuleExecution = ruleExecutionMode;
        }

        public RuleExecutionMode? RuleExecution { get; }

        public BuiltinRulePreference? BuiltinRules { get; }

        public IReadOnlyList<string> RulePaths { get; }

        public IReadOnlyDictionary<string, IRuleConfiguration> RuleConfiguration { get; }
    }

    public class JsonRuleConfiguration : LazyConvertedRuleConfiguration<JObject>
    {
        public JsonRuleConfiguration(
            CommonConfiguration commonConfiguration,
            JObject configurationJson)
            : base(commonConfiguration, configurationJson)
        {
        }

        public override bool TryConvertObject(Type type, JObject configuration, out IRuleConfiguration result)
        {
            try
            {
                result = (IRuleConfiguration)configuration.ToObject(type);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
