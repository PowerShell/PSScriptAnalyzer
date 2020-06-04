using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuleExecutionMode
    {
        [EnumMember(Value = "default")]
        Default = 0,

        [EnumMember(Value = "parallel")]
        Parallel = 1,

        [EnumMember(Value = "sequential")]
        Sequential = 2,
    }

    public interface IScriptAnalyzerConfiguration
    {
        BuiltinRulePreference? BuiltinRules { get; }

        RuleExecutionMode? RuleExecution { get; }

        IReadOnlyList<string> RulePaths { get; }

        IReadOnlyDictionary<string, IRuleConfiguration> RuleConfiguration { get; }
    }
}
