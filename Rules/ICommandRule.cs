using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an interface for a analyzer rule that analyzes the CommandInfo at the given extent.
    /// </summary>
    public interface ICommandRule : IRule
    {
        /// <summary>
        /// AnalyzeCommand: Analyzes a CommandInfo at the given position in the script.
        /// </summary>
        /// <param name="commandInfo">The CommandInfo to be analyzed</param>
        /// <param name="extent">The position in the script</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        IEnumerable<DiagnosticRecord> AnalyzeCommand(CommandInfo commandInfo, IScriptExtent extent, string fileName);
    }
}
