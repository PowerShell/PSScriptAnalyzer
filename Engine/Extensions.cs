using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    // TODO Add documentation
    public static class Extensions
    {
        public static IEnumerable<string> GetLines(this string text)
        {
            return text.Split('\n').Select(line => line.TrimEnd('\r'));
        }

        /// <summary>
        /// Converts IScriptExtent to Range
        /// </summary>
        public static Range ToRange(this IScriptExtent extent)
        {
           return new Range(
                extent.StartLineNumber,
                extent.StartColumnNumber,
                extent.EndLineNumber,
                extent.EndColumnNumber);
        }
    }
}
