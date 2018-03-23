// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides an interface for writing output to a PowerShell session.
    /// </summary>
    public interface IOutputWriter
    {
        /// <summary>
        /// Writes an error to the session.
        /// </summary>
        /// <param name="error">The ErrorRecord to write.</param>
        void WriteError(ErrorRecord error);

        /// <summary>
        /// Writes a warning to the session.
        /// </summary>
        /// <param name="message">The warning string to write.</param>
        void WriteWarning(string message);

        /// <summary>
        /// Writes a verbose message to the session.
        /// </summary>
        /// <param name="message">The verbose message to write.</param>
        void WriteVerbose(string message);

        /// <summary>
        /// Writes a debug message to the session.
        /// </summary>
        /// <param name="message">The debug message to write.</param>
        void WriteDebug(string message);

        /// <summary>
        /// Throws a terminating error in the session.
        /// </summary>
        /// <param name="record">The ErrorRecord which describes the failure.</param>
        void ThrowTerminatingError(ErrorRecord record);
    }
}
