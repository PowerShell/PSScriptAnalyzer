using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    public static class Extensions
    {
       public static bool Contains(this IScriptExtent extentOuter, IScriptExtent extentInner)
        {
            return extentOuter.StartLineNumber <= extentOuter.StartLineNumber
                && extentOuter.EndLineNumber >= extentInner.EndLineNumber
                && extentOuter.StartColumnNumber <= extentInner.StartColumnNumber
                && extentOuter.EndColumnNumber >= extentInner.EndColumnNumber;
        }
    }
}
