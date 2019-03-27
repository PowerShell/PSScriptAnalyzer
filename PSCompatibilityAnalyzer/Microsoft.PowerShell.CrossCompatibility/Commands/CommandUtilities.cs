using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    internal static class CommandUtilities
    {
        private const string COMPATIBILITY_ERROR_ID = "CompatibilityAnalysisError";

        public const string MODULE_PREFIX = "PSCompatibility";

        public static ErrorRecord CreateCompatibilityErrorRecord(
            Exception e,
            string errorId = COMPATIBILITY_ERROR_ID,
            ErrorCategory errorCategory = ErrorCategory.ReadError,
            object targetObject = null)
        {
            return new ErrorRecord(
                e,
                errorId,
                errorCategory,
                targetObject);
        }
    }
}