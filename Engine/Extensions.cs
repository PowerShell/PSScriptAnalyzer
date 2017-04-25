using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    // TODO Add documentation
    public static class Extensions
    {
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
