using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Reflection;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.ScriptAnalyzer.Commands
{
    [Cmdlet(VerbsCommunications.Write, "Diagnostic")]
    [OutputType(typeof(ScriptDiagnostic))]
    public class WriteDiagnosticCommand : PSCmdlet
    {
        private RuleInfo _rule;

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public IScriptExtent[] Extent { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string[] Message { get; set; }

        [Parameter]
        public DiagnosticSeverity? Severity { get; set; }

        protected override void BeginProcessing()
        {
            _rule = GetRule();
        }

        protected override void ProcessRecord()
        {
            for (int i = 0; i < Message.Length; i++)
            {
                WriteObject(new ScriptDiagnostic(_rule, Message[i], Extent[i], Severity ?? _rule?.DefaultSeverity ?? DiagnosticSeverity.Warning));
            }
        }

        private RuleInfo GetRule()
        {
            Debugger debugger = Runspace.DefaultRunspace.Debugger;

            foreach (CallStackFrame frame in debugger.GetCallStack())
            {
                if (!(frame.InvocationInfo.MyCommand is FunctionInfo function))
                {
                    continue;
                }

                if (RuleInfo.TryGetFromFunctionInfo(function, out RuleInfo ruleInfo))
                {
                    return ruleInfo;
                }
            }

            return null;
        }
    }
}
