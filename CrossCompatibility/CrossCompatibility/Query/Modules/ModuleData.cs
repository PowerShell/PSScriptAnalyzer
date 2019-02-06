// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a PowerShell module data object.
    /// </summary>
    public class ModuleData
    {
        private readonly Modules.ModuleData _moduleData;

        /// <summary>
        /// Create a query object around a module data object.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="version">The version of the module.</param>
        /// <param name="moduleData">The module data object.</param>
        public ModuleData(string name, Version version, Modules.ModuleData moduleData)
        {
            _moduleData = moduleData;

            Name = name;
            Version = version;
            Functions = moduleData.Functions?.ToDictionary(f => f.Key, f => new FunctionData(f.Value, f.Key));
            Cmdlets = moduleData.Cmdlets?.ToDictionary(c => c.Key, c => new CmdletData(c.Key, c.Value));
            Aliases = moduleData.Aliases?.ToDictionary(a => a.Key, a => a.Value);
        }

        /// <summary>
        /// The name of the module.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of the module.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The GUID of the module.
        /// </summary>
        public Guid Guid => _moduleData.Guid;

        /// <summary>
        /// Functions exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionData> Functions { get; }

        /// <summary>
        /// Cmdlets exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, CmdletData> Cmdlets { get; }

        /// <summary>
        /// Variables exported by the module.
        /// </summary>
        public IReadOnlyList<string> Variables => _moduleData.Variables;

        /// <summary>
        /// Aliases exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, string> Aliases { get; }
    }
}