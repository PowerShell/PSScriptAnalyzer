using System.Collections.Generic;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class ParameterSetData
    {
        private readonly Modules.ParameterSetData _parameterSet;

        public ParameterSetData(Modules.ParameterSetData parameterSetData, string name)
        {
            Name = name;
            _parameterSet = parameterSetData;
        }

        public string Name { get; }

        public IReadOnlyCollection<Modules.ParameterSetFlag> Flags => _parameterSet.Flags;

        public int Position => _parameterSet.Position;
    }
}