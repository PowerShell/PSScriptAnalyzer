using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class ParameterData
    {
        private readonly Modules.ParameterData _parameter;

        public ParameterData(string name, Modules.ParameterData parameterData)
        {
            ParameterSets = parameterData.ParameterSets.ToDictionary(p => p.Key, p => new ParameterSetData(p.Key, p.Value));
            Name = name;
        }

        public IReadOnlyDictionary<string, ParameterSetData> ParameterSets { get; }

        public string Type => _parameter.Type;

        public bool IsDynamic => _parameter.Dynamic;

        public string Name { get; }
    }
}