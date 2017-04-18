using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    // TODO Add documentation
    public static class Extensions
    {
        public static bool Contains(this IScriptExtent extentOuter, IScriptExtent extentInner)
        {
            return extentOuter.StartLineNumber <= extentOuter.StartLineNumber
                && extentOuter.EndLineNumber >= extentInner.EndLineNumber
                && extentOuter.StartColumnNumber <= extentInner.StartColumnNumber
                && extentOuter.EndColumnNumber >= extentInner.EndColumnNumber;
        }

        public static IEnumerable<string> GetLines(this string text)
        {
            var lines = new List<string>();
            using (var stringReader = new StringReader(text))
            {
                string line;
                line = stringReader.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = stringReader.ReadLine();
                }
            }
        }
    }
}
