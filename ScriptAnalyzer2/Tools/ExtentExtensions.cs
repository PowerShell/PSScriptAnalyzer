using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Tools
{
    public static class ExtentExtensions
    {
        public static bool Contains(this IScriptExtent thisExtent, IScriptExtent thatExtent)
        {
            return thisExtent.StartOffset <= thatExtent.StartOffset
                && thisExtent.EndOffset >= thatExtent.EndOffset;
        }
    }
}
