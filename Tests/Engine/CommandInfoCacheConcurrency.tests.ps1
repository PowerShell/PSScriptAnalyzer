# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Concurrent command lookups" {
    BeforeAll {
        # Run the analyzer once so that the singleton Helper is created by the cmdlet. Touching
        # Helper.Instance before that would install a helper without a command invocation context,
        # which breaks every later analysis in this process.
        $null = Invoke-ScriptAnalyzer -ScriptDefinition 'Get-Item -Path .'

        # The concurrency driver is written in C# so that the lookups really do run on separate
        # threads. Invoking a PowerShell script block on a thread pool thread would introduce
        # runspace affinity problems of its own and would not test the command info cache.
        $analyzerAssembly = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper].Assembly.Location
        $references = @($analyzerAssembly, ([System.Management.Automation.PSObject].Assembly.Location))
        if ($PSVersionTable.PSEdition -eq 'Core') {
            $references += Join-Path $PSHOME 'ref/System.Collections.dll'
        }
        Add-Type -IgnoreWarnings -WarningAction SilentlyContinue -ReferencedAssemblies $references -TypeDefinition @'
using System.Threading.Tasks;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;

public static class ConcurrentCommandLookup
{
    public static void ResolveExports()
    {
        Token[] tokens;
        ParseError[] errors;
        var ast = Parser.ParseInput("Export-ModuleMember -Function Test-Example", out tokens, out errors);
        var helper = Helper.Instance;
        var tasks = new Task[8];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    helper.GetCommandInfo("Get-Command", bypassCache: true);
                    var parameters = helper.GetCommandInfo("Get-Item").Parameters;
                    var exports = helper.GetExportedFunction(ast);
                    if (!exports.SetEquals(new[] { "Test-Example" }))
                    {
                        throw new System.InvalidOperationException("Exported function was not resolved.");
                    }
                }
            });
        }

        Task.WaitAll(tasks);
    }

    public static string[] Lookup(string[] commandNames)
    {
        var helper = Helper.Instance;
        var tasks = new Task<string>[commandNames.Length];
        for (int i = 0; i < commandNames.Length; i++)
        {
            string name = commandNames[i];
            tasks[i] = Task.Run(() =>
            {
                var commandInfo = helper.GetCommandInfo(name);
                return commandInfo == null ? null : commandInfo.Name;
            });
        }

        Task.WaitAll(tasks);

        var results = new string[tasks.Length];
        for (int i = 0; i < tasks.Length; i++)
        {
            results[i] = tasks[i].Result;
        }

        return results;
    }
}
'@
    }

    It "resolves commands from several threads without failing" {
        $commandNames = @(
            'Get-ChildItem', 'Where-Object', 'ForEach-Object', 'Get-Content', 'Write-Output',
            'Test-Path', 'Get-Command', 'Select-Object', 'Sort-Object', 'Measure-Object'
        ) * 4

        # A lookup that hits the thread safety problem throws, which fails the test.
        $results = [ConcurrentCommandLookup]::Lookup($commandNames)

        $results.Count | Should -Be $commandNames.Count
        # A failed lookup returns null, so every entry must name the command that was requested.
        for ($i = 0; $i -lt $commandNames.Count; $i++) {
            $results[$i] | Should -BeExactly $commandNames[$i]
        }

    }

    It "resolves exported functions while command lookups run concurrently" {
        [ConcurrentCommandLookup]::ResolveExports()
    }
}
