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

        public static IScriptExtent Translate(this IScriptExtent extent, int lineDelta, int columnDelta)
        {
            var newStartLineNumber = extent.StartLineNumber + lineDelta;
            if (newStartLineNumber < 1)
            {
                throw new ArgumentException(
                    "Invalid line delta. Resulting start line number must be greather than 1.");
            }

            var newStartColumnNumber = extent.StartColumnNumber + columnDelta;
            var newEndColumnNumber = extent.EndColumnNumber + columnDelta;
            if (newStartColumnNumber < 1 || newEndColumnNumber < 1)
            {
                throw new ArgumentException(@"Invalid column delta.
Resulting start column and end column number must be greather than 1.");
            }

            return new ScriptExtent(
                new ScriptPosition(
                    extent.File,
                    newStartLineNumber,
                    newStartColumnNumber,
                    extent.StartScriptPosition.Line),
                new ScriptPosition(
                    extent.File,
                    extent.EndLineNumber + lineDelta,
                    newEndColumnNumber,
                    extent.EndScriptPosition.Line));
        }
    }
}
