using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Rules
{
    public enum SourceType
    {
        Builtin = 0,
        Assembly = 1,
        PowerShellModule = 2,
    }
}
