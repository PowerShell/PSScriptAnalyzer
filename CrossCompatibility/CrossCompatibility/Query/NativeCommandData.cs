using System;
using System.Collections.Generic;
using System.IO;
using NativeCommandDataMut = Microsoft.PowerShell.CrossCompatibility.Data.NativeCommandData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class NativeCommandData
    {
        public NativeCommandData(string name, NativeCommandDataMut nativeCommandMut)
        {
            Name = name;
            Version = nativeCommandMut?.Version;
            Path = nativeCommandMut?.Path;
        }

        public string Name { get; }

        public Version Version { get; }

        public string Path { get; }
    }

    public class NativeCommandLookupTable
    {
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

        private NativeCommandLookupTable(IReadOnlyDictionary<string, IReadOnlyList<NativeCommandData>> nativeCommandTable)
        {
            _nativeCommands = nativeCommandTable;
        }

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