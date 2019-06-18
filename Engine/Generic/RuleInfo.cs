// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an internal class to properly display the name and description of a rule.
    /// </summary>
    public class RuleInfo
    {
        private string name;
        private string commonName;
        private string description;
        private SourceType sourceType;
        private string sourceName;
        private RuleSeverity ruleSeverity;
        private Type implementingType;

        /// <summary>
        /// Name: The name of the rule.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string RuleName
        {
            get { return name; }
            private set { name = value; }
        }

        /// <summary>
        /// Name: The common name of the rule.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CommonName
        {
            get { return commonName; }
            private set { commonName = value; }
        }

        /// <summary>
        /// Description: The description of the rule.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Description
        {
            get { return description; }
            private set { description = value; }
        }

        /// <summary>
        /// SourceType: The source type of the rule.
        /// </summary>
        ///
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public SourceType SourceType
        {
            get { return sourceType; }
            private set { sourceType = value; }
        }

        /// <summary>
        /// SourceName : The source name of the rule.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string SourceName
        {
            get { return sourceName; }
            private set { sourceName = value; }
        }

        /// <summary>
        /// Severity : The severity of the rule violation.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public RuleSeverity Severity
        {
            get { return ruleSeverity; }
            private set { ruleSeverity = value; }
        }

        /// <summary>
        /// ImplementingType : The type which implements the rule.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Type ImplementingType
        {
            get { return implementingType; }
            private set { implementingType = value; }
        }

        /// <summary>
        /// Constructor for a RuleInfo.
        /// </summary>
        /// <param name="name">Name of the rule.</param>
        /// <param name="commonName">Common Name of the rule.</param>
        /// <param name="description">Description of the rule.</param>
        /// <param name="sourceType">Source type of the rule.</param>
        /// <param name="sourceName">Source name of the rule.</param>
        public RuleInfo(string name, string commonName, string description, SourceType sourceType, string sourceName, RuleSeverity severity)
        {
            RuleName        = name;
            CommonName  = commonName;
            Description = description;
            SourceType  = sourceType;
            SourceName  = sourceName;
            Severity = severity;
        }

        /// <summary>
        /// Constructor for a RuleInfo.
        /// </summary>
        /// <param name="name">Name of the rule.</param>
        /// <param name="commonName">Common Name of the rule.</param>
        /// <param name="description">Description of the rule.</param>
        /// <param name="sourceType">Source type of the rule.</param>
        /// <param name="sourceName">Source name of the rule.</param>
        /// <param name="implementingType">The dotnet type of the rule.</param>
        public RuleInfo(string name, string commonName, string description, SourceType sourceType, string sourceName, RuleSeverity severity, Type implementingType)
        {
            RuleName        = name;
            CommonName  = commonName;
            Description = description;
            SourceType  = sourceType;
            SourceName  = sourceName;
            Severity = severity;
            ImplementingType = implementingType;
        }

        public override string ToString()
        {
            return RuleName;
        }
    }
}
