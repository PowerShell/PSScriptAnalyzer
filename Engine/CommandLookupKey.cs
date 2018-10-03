// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    internal struct CommandLookupKey : IEquatable<CommandLookupKey>
    {
        private readonly string Name;

        private readonly CommandTypes CommandTypes;

        internal CommandLookupKey(string name, CommandTypes? commandTypes)
        {
            Name = name;
            CommandTypes = commandTypes ?? CommandTypes.All;
        }

        public bool Equals(CommandLookupKey other)
        {
            return CommandTypes == other.CommandTypes
                && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            // Algorithm from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Name.ToUpperInvariant().GetHashCode();
                hash = hash * 31 + CommandTypes.GetHashCode();
                return hash;
            }
        }
    }
}
