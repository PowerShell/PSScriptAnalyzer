// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// Denotes an exception that occurs while analyzing a PowerShell platform
    /// or compatibility with one.
    /// </summary>
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
