using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Commands;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// ILogger: An interface for a PSScriptAnalyzer logger to output the results of PSScriptAnalyzer rules.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// LogMessage: Logs the given diagnostic, using the command for Write methods if needed.
        /// </summary>
        /// <param name="obj">The object to be logged.</param>
        /// <param name="command">The InvokePSScriptAnalyzerCommand that this logger is running off of.</param>
        void LogObject(Object obj, InvokeScriptAnalyzerCommand command);

        /// <summary>
        /// GetName: Retrieves the name of the logger.
        /// </summary>
        /// <returns>The name of the logger</returns>
        string GetName();

        /// <summary>
        /// GetDescription: Retrives the description of the logger.
        /// </summary>
        /// <returns>The description of the logger</returns>
        string GetDescription();
    }
}
