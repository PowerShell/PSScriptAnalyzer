using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class ParameterData
    {
        private readonly Modules.ParameterData _parameter;

        public ParameterData(Modules.ParameterData parameterData, string name)
        {
            ParameterSets = parameterData.ParameterSets.ToDictionary(p => p.Key, p => new ParameterSetData(p.Value, p.Key));
            Name = name;
        }

        public IReadOnlyDictionary<string, ParameterSetData> ParameterSets { get; }

        public string Type => _parameter.Type;

        public bool IsDynamic => _parameter.Dynamic;

        public string Name { get; }
    }
}