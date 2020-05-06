using System;

namespace Microsoft.PowerShell.ScriptAnalyzer
{
    public class ScriptAnalyzerException : Exception
    {
        protected ScriptAnalyzerException() : base()
        {
        }

        public ScriptAnalyzerException(string message) : base(message)
        {
        }

        public ScriptAnalyzerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ScriptAnalyzerConfigurationException : ScriptAnalyzerException
    {
        public ScriptAnalyzerConfigurationException() : base()
        {
        }

        public ScriptAnalyzerConfigurationException(string message) : base(message)
        {
        }

        public ScriptAnalyzerConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ConfigurationNotFoundException : ScriptAnalyzerConfigurationException
    {
        public ConfigurationNotFoundException() : base()
        {
        }

        public ConfigurationNotFoundException(string message) : base(message)
        {
        }
    }
}
