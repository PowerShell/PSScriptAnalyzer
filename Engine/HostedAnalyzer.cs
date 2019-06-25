using System;
using SMA = System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.IO;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public class HostedAnalyzer
    {
        private SMA.PowerShell ps;
        private IOutputWriter writer;
        private ScriptAnalyzer analyzer;
        /// <summary>
        /// Create an instance of the analyzer
        /// </summary>
        public HostedAnalyzer()
        {
            ps = SMA.PowerShell.Create();
            writer = new StreamStorage();
            ScriptAnalyzer.Instance.Initialize(ps.Runspace, writer);
            analyzer = ScriptAnalyzer.Instance;
        }

        /// <summary>Analyze a script in the form of a string</summary>
        public IEnumerable<DiagnosticRecord> Analyze(string ScriptDefinition)
        {
            var diagnosticList = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
            return diagnosticList;
        }

        /// <summary>Analyze a script in the form of a file</summary>
        public IEnumerable<DiagnosticRecord> Analyze(FileInfo File)
        {
            return default(IEnumerable<DiagnosticRecord>);
        }
    }

    public class AnalyzerResult
    {

    }
}