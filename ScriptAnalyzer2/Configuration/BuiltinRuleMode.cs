
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BuiltinRulePreference
    {
        [EnumMember(Value = "none")]
        None = 0,

        [EnumMember(Value = "default")]
        Default,

        [EnumMember(Value = "comprehensive")]
        Aggressive,
    }
}
