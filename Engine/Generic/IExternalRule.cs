using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an interface for an external analyzer rule.
    /// </summary>
    internal interface IExternalRule : IRule
    {
        /// <summary>
        /// GetParameter: Retrieves AstType parameter
        /// </summary>
        /// <returns>string</returns>
        string GetParameter();

        /// <summary>
        /// GetFullModulePath: Retrieves full module path.
        /// </summary>
        /// <returns>string</returns>
        string GetFullModulePath();
    }
}
