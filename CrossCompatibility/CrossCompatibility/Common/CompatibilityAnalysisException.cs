using System;

namespace Microsoft.PowerShell.CrossCompatibility
{
    public class CompatibilityAnalysisException : Exception
    {
        public CompatibilityAnalysisException(string message) : base(message)
        {
        }

        public CompatibilityAnalysisException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}