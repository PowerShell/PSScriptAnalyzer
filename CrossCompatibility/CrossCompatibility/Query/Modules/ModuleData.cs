using System;
using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class ModuleData
    {
        private readonly Modules.ModuleData _moduleData;

        public ModuleData(string name, Version version, Modules.ModuleData moduleData)
        {
            _moduleData = moduleData;

            Name = name;
            Version = version;
            Functions = moduleData.Functions.ToDictionary(f => f.Key, f => new FunctionData(f.Value, f.Key));
            Cmdlets = moduleData.Cmdlets.ToDictionary(c => c.Key, c => new CmdletData(c.Key, c.Value));
            Aliases = moduleData.Aliases.ToDictionary(a => a.Key, a => a.Value);
        }

        public string Name { get; }

        public Version Version { get; }

        public Guid Guid => _moduleData.Guid;

        public IReadOnlyDictionary<string, FunctionData> Functions { get; }

        public IReadOnlyDictionary<string, CmdletData> Cmdlets { get; }

        public IReadOnlyList<string> Variables => _moduleData.Variables;

        public IReadOnlyDictionary<string, string> Aliases { get; }
    }
}