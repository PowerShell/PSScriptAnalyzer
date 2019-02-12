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

            var functions = new Dictionary<string, FunctionData>(StringComparer.OrdinalIgnoreCase);
            var cmdlets = new Dictionary<string, CmdletData>(StringComparer.OrdinalIgnoreCase);
            var aliases = new Dictionary<string, CommandData>(StringComparer.OrdinalIgnoreCase);

            if (moduleData.Functions != null)
            {
                foreach (KeyValuePair<string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.FunctionData> function in moduleData.Functions)
                {
                    functions.Add(function.Key, new FunctionData(function.Key, function.Value));
                }
            }

            if (moduleData.Cmdlets != null)
            {
                foreach (KeyValuePair<string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.CmdletData> cmdlet in moduleData.Cmdlets)
                {
                    cmdlets.Add(cmdlet.Key, new CmdletData(cmdlet.Key, cmdlet.Value));
                }
            }

            if (moduleData.Aliases != null)
            {
                foreach (KeyValuePair<string, string> alias in moduleData.Aliases)
                {
                    if (cmdlets.TryGetValue(alias.Value, out CmdletData cmdlet))
                    {
                        aliases.Add(alias.Key, cmdlet);
                        continue;
                    }

                    if (functions.TryGetValue(alias.Value, out FunctionData function))
                    {
                        aliases.Add(alias.Key, function);
                        continue;
                    }
                }
            }

            Functions = functions;
            Cmdlets = cmdlets;
            Aliases = aliases;
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
        public IReadOnlyDictionary<string, CommandData> Aliases { get; }
    }
}