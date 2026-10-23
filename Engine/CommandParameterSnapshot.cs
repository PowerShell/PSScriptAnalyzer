// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>Detached parameter facts, safe to consume without driving a PowerShell runspace.</summary>
    public sealed class CommandParameterSnapshot
    {
        public string Name { get; }
        public bool SwitchParameter { get; }
        public ReadOnlyCollection<string> Aliases { get; }

        internal CommandParameterSnapshot(ParameterMetadata parameter)
        {
            Name = parameter.Name;
            SwitchParameter = parameter.SwitchParameter;
            Aliases = new ReadOnlyCollection<string>(parameter.Aliases.ToArray());
        }
    }
}
