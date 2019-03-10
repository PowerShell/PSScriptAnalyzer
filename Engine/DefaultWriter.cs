using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public class SensibleDefaults
    {
        public StreamStorage Streams;
        public Runspace Runspace;
        private System.Management.Automation.PowerShell ps;
        public SensibleDefaults()
        {
           Streams = new StreamStorage();
           ps = System.Management.Automation.PowerShell.Create();
           Runspace = ps.Runspace;
        }
        public void Reset()
        {
            if ( ps != null )
            {
                if ( Runspace != null )
                {
                    Runspace.Dispose();
                }
                ps.Dispose();
            }
            if ( Streams != null )
            {
                Streams.Dispose();
            }
        }
    }

    public class StreamStorage : IOutputWriter
    {
        public StreamStorage()
        {
            TerminatingErrors = new List<ErrorRecord>();
            Errors = new List<ErrorRecord>();
            Debug = new List<string>();
            Verbose = new List<string>();
            Warning = new List<string>();
        }
        public void Dispose()
        {
        }
        public List<ErrorRecord> TerminatingErrors;
        public List<ErrorRecord> Errors;
        public List<string>Debug;
        public List<string>Verbose;
        public List<string>Warning;
        public void WriteError(ErrorRecord r) { Errors.Add(r); }
        public void ThrowTerminatingError(ErrorRecord r) { TerminatingErrors.Add(r); }
        public void WriteDebug(string m) { Debug.Add(m); }
        public void WriteVerbose(string m) { Verbose.Add(m); }
        public void WriteWarning(string m) { Warning.Add(m); }
        public void Clear()
        {
            TerminatingErrors.Clear();
            Errors.Clear();
            Debug.Clear();
            Verbose.Clear();
            Warning.Clear();
        }
    }
}
