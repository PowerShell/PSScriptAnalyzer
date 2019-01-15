using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class ParameterData
    {
        private readonly Modules.ParameterData _parameterData;

        public ParameterData(string name, Modules.ParameterData parameterData)
        {
            _parameterData = parameterData;
            ParameterSets = parameterData.ParameterSets?.ToDictionary(p => p.Key, p => new ParameterSetData(p.Key, p.Value));
            Name = name;
        }

        public IReadOnlyDictionary<string, ParameterSetData> ParameterSets { get; }

        public string Type => _parameterData.Type;

        public bool IsDynamic => _parameterData.Dynamic;

        public string Name { get; }
    }
}