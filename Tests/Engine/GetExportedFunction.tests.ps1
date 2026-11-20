# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    # Sets Helper.Instance as a run of Invoke-ScriptAnalyzer does.
    $null = Invoke-ScriptAnalyzer -ScriptDefinition 'Get-ChildItem'

    function Get-ExportedFunction([string] $ScriptDefinition) {
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($ScriptDefinition, [ref] $null, [ref] $null)
        , [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::Instance.GetExportedFunction($ast)
    }

    # The rules AvoidReservedCharInCmdlet and ProvideCommentHelp call GetExportedFunction at the
    # same time, on the threads of one analysis, while other rules look up commands in the
    # runspaces of the command cache. The type starts its callers at one barrier, and keeps the
    # runspaces busy with lookups until the callers end.
    $references = @([Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper].Assembly.Location)
    if ($PSVersionTable.PSEdition -eq 'Core') {
        $references += [psobject].Assembly.Location, 'System.Runtime', 'System.Collections', 'System.Collections.Concurrent', 'System.Threading', 'System.Threading.Thread'
    }
    Add-Type -IgnoreWarnings -WarningAction SilentlyContinue -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Collections.Concurrent;
using System.Management.Automation.Language;
using System.Threading;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;

public static class GetExportedFunctionStress
{
    public static string[] Run(Ast ast, string expectedFunction, int callerCount, int callsPerCaller, int lookupCount)
    {
        var failures = new ConcurrentQueue<string>();
        var start = new Barrier(callerCount + lookupCount);
        int runningCallers = callerCount;
        var threads = new Thread[callerCount + lookupCount];
        for (int t = 0; t < callerCount; t++)
        {
            threads[t] = new Thread(() =>
            {
                start.SignalAndWait();
                for (int i = 0; i < callsPerCaller; i++)
                {
                    try
                    {
                        if (!Helper.Instance.GetExportedFunction(ast).Contains(expectedFunction))
                        {
                            failures.Enqueue("The exported functions miss " + expectedFunction);
                        }
                    }
                    catch (Exception e)
                    {
                        failures.Enqueue(e.GetType().FullName + ": " + e.Message + " " + TopFrame(e));
                    }
                }

                Interlocked.Decrement(ref runningCallers);
            });
        }

        for (int t = callerCount; t < threads.Length; t++)
        {
            threads[t] = new Thread(() =>
            {
                start.SignalAndWait();
                while (Volatile.Read(ref runningCallers) > 0)
                {
                    try
                    {
                        Helper.Instance.GetCommandInfo("Get-ChildItem", null, true);
                    }
                    catch (Exception e)
                    {
                        failures.Enqueue("Lookup: " + e.GetType().FullName + ": " + e.Message + " " + TopFrame(e));
                    }
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return failures.ToArray();
    }

    private static string TopFrame(Exception e)
    {
        string trace = e.StackTrace ?? string.Empty;
        int end = trace.IndexOf((char)10);
        return (end < 0 ? trace : trace.Substring(0, end)).Trim();
    }
}
'@
}

Describe 'GetExportedFunction' {
    It 'reads the functions of <Call>' -TestCases @(
        @{ Call = 'Export-ModuleMember -Function Get-Widget' }
        @{ Call = 'Export-ModuleMember -Function:Get-Widget' }
        @{ Call = 'Export-ModuleMember -Fun Get-Widget' }
        @{ Call = 'Export-ModuleMember Get-Widget' }
        @{ Call = 'Export-ModuleMember -Alias gw -Function Get-Widget' }
        @{ Call = 'Export-ModuleMember -Verbose -Function Get-Widget' }
        @{ Call = 'Export-ModuleMember -NoSuchParameter -Function Get-Widget' }
    ) {
        Get-ExportedFunction "function Get-Widget {}`n$Call" | Should -Be @('Get-Widget')
    }

    It 'gives each of many parallel callers the exported functions' {
        $ast = [System.Management.Automation.Language.Parser]::ParseInput(
            "function Get-Widget {}`nExport-ModuleMember -Function Get-Widget",
            [ref] $null, [ref] $null)

        # A round can end before the race occurs, so the test runs several.
        $failures = foreach ($round in 1..5) {
            [GetExportedFunctionStress]::Run($ast, 'Get-Widget', 16, 1000, 16)
        }

        $failures | Select-Object -Unique | Should -BeNullOrEmpty
    }
}
