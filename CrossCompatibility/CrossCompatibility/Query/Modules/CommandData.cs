using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public abstract class CommandData
    {
        protected readonly Modules.CommandData _commandData;

        protected CommandData(string name, Modules.CommandData commandData)
        {
            _commandData = commandData;
            ParameterAliases = commandData.ParameterAliases?.ToDictionary(a => a.Key, a => a.Value);
            Parameters = _commandData.Parameters?.ToDictionary(p => p.Key, p => new ParameterData(p.Key, p.Value));
            Name = name;
        }

        public IReadOnlyList<string> OutputType => _commandData.OutputType;

        public IReadOnlyList<string> ParameterSets => _commandData.ParameterSets;

        public string DefaultParameterSet => _commandData.DefaultParameterSet;

        public IReadOnlyDictionary<string, string> ParameterAliases { get; }

        public IReadOnlyDictionary<string, ParameterData> Parameters { get; }

        public string Name { get; }

        public abstract bool IsCmdletBinding { get; }
    }
}