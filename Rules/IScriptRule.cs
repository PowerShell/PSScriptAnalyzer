using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Language;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an interface for an analyzer rule that analyzes the Ast.
    /// </summary>
    public interface IScriptRule : IRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the given Ast and returns DiagnosticRecords based on the anaylsis.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName);
    }
}
