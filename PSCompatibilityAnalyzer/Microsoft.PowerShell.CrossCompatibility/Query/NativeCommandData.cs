// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using NativeCommandDataMut = Microsoft.PowerShell.CrossCompatibility.Data.NativeCommandData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a native/application command/util available to PowerShell.
    /// </summary>
    public class NativeCommandData
    {
        /// <summary>
        /// Create a new query object around collected native command data.
        /// </summary>
        /// <param name="name">The name of the native command.</param>
        /// <param name="nativeCommandMut">Data describing a native command collected from a PowerShell runtime.</param>
        public NativeCommandData(string name, NativeCommandDataMut nativeCommandMut)
        {
            Name = name;
            Version = nativeCommandMut?.Version;
            Path = nativeCommandMut?.Path;
        }

        /// <summary>
        /// The invocation name of the native command, stripped of any extension.
        /// For example 'dir', 'powershell', 'ls'.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of the command if known.
        /// A version of 0.0.0.0 should be considered the same as null/no version.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The full path of the command, including its extension.
        /// </summary>
        public string Path { get; }
    }

    /// <summary>
    /// Special lookup table for native commands, to support target-dependent case sensitivity.
    /// </summary>
    public class NativeCommandLookupTable
    {
        /// <summary>
        /// Create a new native command lookup table.
        /// </summary>
        /// <param name="nativeCommands">A list of native command data objects collected from a PowerShell runtime.</param>
        /// <returns>A new lookup table to query native commands.</returns>
        public static NativeCommandLookupTable Create(IReadOnlyDictionary<string, NativeCommandDataMut[]> nativeCommands)
        {
            var table = new Dictionary<string, IReadOnlyList<NativeCommandData>>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, NativeCommandDataMut[]> entry in nativeCommands)
            {
                var entryList = new List<NativeCommandData>();
                foreach (NativeCommandDataMut nativeCommand in entry.Value)
                {
                    string commandName = nativeCommand.Path != null ? Path.GetFileName(nativeCommand.Path) : entry.Key;
                    entryList.Add(new NativeCommandData(commandName, nativeCommand));
                }

                // Add native commands to the table with the extension stripped out of the key
                table[Path.GetFileNameWithoutExtension(entry.Key)] = entryList.ToArray();
            }

            return new NativeCommandLookupTable(table);
        }

        private readonly IReadOnlyDictionary<string, IReadOnlyList<NativeCommandData>> _nativeCommands;

        /// <summary>
        /// Private constructor for the native command lookup table from a dictionary.
        /// </summary>
        /// <param name="nativeCommandTable">The internal native command dictionary used to back the lookup table.</param>
        private NativeCommandLookupTable(IReadOnlyDictionary<string, IReadOnlyList<NativeCommandData>> nativeCommandTable)
        {
            _nativeCommands = nativeCommandTable;
        }

        /// <summary>
        /// Check if a command is defined in the lookup table.
        /// </summary>
        /// <param name="commandName">The name of the command to search for.</param>
        /// <param name="caseSensitive">Whether to search case-sensitively or not.</param>
        /// <returns>True if the command is defined, false otherwise.</returns>
        public bool HasCommand(string commandName, bool caseSensitive = true)
        {
            commandName = Path.GetFileNameWithoutExtension(commandName);

            if (!_nativeCommands.TryGetValue(commandName, out IReadOnlyList<NativeCommandData> matchedCommands))
            {
                return false;
            }

            if (!caseSensitive)
            {
                return true;
            }

            foreach (NativeCommandData command in matchedCommands)
            {
                if (command.Name.Equals(commandName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to find a native command by name in the lookup table.
        /// </summary>
        /// <param name="commandName">The name of the native command to find.</param>
        /// <param name="matchedCommands">The list of all commands matched.</param>
        /// <param name="caseSensitive">Whether to search for commands case-sensitively or not.</param>
        /// <returns>True if the command was found and the matchedCommands field was populated, false otherwise.</returns>
        public bool TryGetCommand(string commandName, out IReadOnlyList<NativeCommandData> matchedCommands, bool caseSensitive = true)
        {
            commandName = Path.GetFileNameWithoutExtension(commandName);

            if (!_nativeCommands.TryGetValue(commandName, out IReadOnlyList<NativeCommandData> allMatchedCommands))
            {
                matchedCommands = null;
                return false;
            }

            if (!caseSensitive)
            {
                matchedCommands = allMatchedCommands;
                return true;
            }

            var caseMatchingCommands = new List<NativeCommandData>();
            foreach (NativeCommandData command in allMatchedCommands)
            {
                if (command.Name.Equals(commandName))
                {
                    caseMatchingCommands.Add(command);
                }
            }

            matchedCommands = caseMatchingCommands;
            return caseMatchingCommands.Count > 0;
        }
    }
}
