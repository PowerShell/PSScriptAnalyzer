using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
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

        /// <summary>
        /// Name: The name of the rule.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Name
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
        /// Constructor for a RuleInfo.
        /// </summary>
        /// <param name="name">Name of the rule.</param>
        /// <param name="commonName">Common Name of the rule.</param>
        /// <param name="description">Description of the rule.</param>
        /// <param name="sourceType">Source type of the rule.</param>
        /// <param name="sourceName">Source name of the rule.</param>
        public RuleInfo(string name, string commonName, string description, SourceType sourceType, string sourceName)
        {
            Name        = name;
            CommonName  = commonName;
            Description = description;
            SourceType  = sourceType;
            SourceName  = sourceName;
        }
    }
}
