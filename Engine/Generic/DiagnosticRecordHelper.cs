using System;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    public static class DiagnosticRecordHelper
    {
        public static string FormatError(string format, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, format, args);
        }
    }
}
