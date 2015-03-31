using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents a source name of a script analyzer rule.
    /// </summary>
    public enum SourceType : uint
    {
        /// <summary>
        /// BUILTIN: Indicates the script analyzer rule is contributed as a built-in rule.
        /// </summary>
        Builtin = 0,

        /// <summary>
        /// MANAGED: Indicates the script analyzer rule is contirbuted as a managed rule.
        /// </summary>
        Managed = 1,

        /// <summary>
        /// MODULE: Indicates the script analyzer rule is contributed as a Windows PowerShell module rule.
        /// </summary>
        Module  = 2,
    };
}
