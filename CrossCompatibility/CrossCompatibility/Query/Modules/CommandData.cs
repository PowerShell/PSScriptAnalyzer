// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// A readonly query object for PowerShell command data.
    /// </summary>
    public abstract class CommandData
    {
        protected readonly Modules.CommandData _commandData;

        /// <summary>
        /// Create a new command data query object from the data object.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="commandData">The command data object describing the command.</param>
        protected CommandData(string name, Modules.CommandData commandData)
        {
            _commandData = commandData;
            ParameterAliases = commandData.ParameterAliases?.ToDictionary(a => a.Key, a => a.Value);
            Parameters = _commandData.Parameters?.ToDictionary(p => p.Key, p => new ParameterData(p.Key, p.Value));
            Name = name;
        }

        /// <summary>
        /// The output types of the command, if any.
        /// </summary>
        public IReadOnlyList<string> OutputType => _commandData.OutputType;

        /// <summary>
        /// The parameter sets of the command, if any.
        /// </summary>
        public IReadOnlyList<string> ParameterSets => _commandData.ParameterSets;

        /// <summary>
        /// The default parameter set of the command, if any.
        /// </summary>
        public string DefaultParameterSet => _commandData.DefaultParameterSet;

        /// <summary>
        /// Parameter aliases of the command.
        /// </summary>
        public IReadOnlyDictionary<string, string> ParameterAliases { get; }

        /// <summary>
        /// Parameters of the command.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> Parameters { get; }

        /// <summary>
        /// The command name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True if this command is bound as a cmdlet (or advanced function), false otherwise.
        /// </summary>
        public abstract bool IsCmdletBinding { get; }
    }
}