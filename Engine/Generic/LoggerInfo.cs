// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an internal class to properly display the name and description of a logger.
    /// </summary>
    internal class LoggerInfo
    {
        private string name;
        private string description;

        /// <summary>
        /// Name: The name of the logger.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Name
        {
            get { return name; }
            private set { name = value; }
        }

        /// <summary>
        /// Description: The description of the logger.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Description
        {
            get { return description; }
            private set { description = value; }
        }

        /// <summary>
        /// Constructor for a LoggerInfo.
        /// </summary>
        /// <param name="name">The name of the logger</param>
        /// <param name="description">The description of the logger</param>
        public LoggerInfo(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
