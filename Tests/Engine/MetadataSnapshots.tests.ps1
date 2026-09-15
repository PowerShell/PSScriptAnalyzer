# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Detached command metadata" {
    BeforeAll {
        $null = Invoke-ScriptAnalyzer -ScriptDefinition 'Write-Output example'
        $assembly = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper].Assembly
        $references = @($assembly.Location, [System.Management.Automation.PSObject].Assembly.Location)
        if ($PSVersionTable.PSEdition -eq 'Core') {
            $references += Join-Path $PSHOME 'ref/System.Collections.dll'
        }
        Add-Type -IgnoreWarnings -WarningAction SilentlyContinue -ReferencedAssemblies $references -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;

[Cmdlet("Test", "SnapshotDynamic")]
public class SnapshotDynamicCommand : PSCmdlet, IDynamicParameters
{
    public static string ParameterName = "First";

    public object GetDynamicParameters()
    {
        var parameters = new RuntimeDefinedParameterDictionary();
        parameters.Add(ParameterName, new RuntimeDefinedParameter(ParameterName, typeof(string),
            new Collection<Attribute> { new ParameterAttribute() }));
        return parameters;
    }
}

[Cmdlet("Test", "SnapshotStatic")]
public class SnapshotStaticCommand : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string Example { get; set; }
}

public class FaultingMetadataInfo : CmdletInfo
{
    public static Exception Failure;
    public static int FailuresRemaining;

    public FaultingMetadataInfo() : base("Test-SnapshotStatic", typeof(SnapshotStaticCommand)) { }

    public override Dictionary<string, ParameterMetadata> Parameters
    {
        get
        {
            if (FailuresRemaining-- > 0) throw Failure;
            return base.Parameters;
        }
    }
}

public static class MetadataSnapshotTests
{
    private static readonly Type CacheType = typeof(Helper).Assembly.GetType(
        "Microsoft.Windows.PowerShell.ScriptAnalyzer.CommandInfoCache");

    // Block bodies instead of expression-bodied members: Windows PowerShell 5.1 compiles
    // Add-Type definitions with the C# 5 compiler.
    public static IDisposable Create()
    {
        return (IDisposable)Activator.CreateInstance(CacheType);
    }

    public static IReadOnlyDictionary<string, CommandParameterSnapshot> Parameters(
        IDisposable cache, string name, bool bypass = false)
    {
        return (IReadOnlyDictionary<string, CommandParameterSnapshot>)CacheType.GetMethod("GetParameterSnapshot")
            .Invoke(cache, new object[] { name, null, bypass });
    }

    public static object Mandatory(IDisposable cache, string name)
    {
        return CacheType.GetMethod("GetMandatoryParameterNames").Invoke(cache, new object[] { name });
    }

    public static object ReadMetadata(IDisposable cache, string name, string method, bool bypass = false)
    {
        var arguments = method == "GetCommandParameterSets" || method == "GetMandatoryParameterNames"
            ? new object[] { name } : new object[] { name, null, bypass };
        return CacheType.GetMethod(method).Invoke(cache, arguments);
    }

    public static void ConcurrentStatic(IDisposable cache)
    {
        var tasks = new Task<IReadOnlyDictionary<string, CommandParameterSnapshot>>[16];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => Parameters(cache, "Export-ModuleMember"));
        }
        Task.WaitAll(tasks);
        foreach (var task in tasks)
        {
            if (!ReferenceEquals(tasks[0].Result, task.Result) || !task.Result.ContainsKey("Function"))
                throw new InvalidOperationException("Static metadata was not shared safely.");
        }
    }

    public static void CheckDetached(IDisposable cache)
    {
        var snapshot = Parameters(cache, "Write-Output");
        var live = (Dictionary<string, ParameterMetadata>)CacheType.GetMethod("GetCommandParameters")
            .Invoke(cache, new object[] { "Write-Output", null, false });
        live["InputObject"].Aliases.Add("SnapshotMustNotChange");
        if (snapshot["InputObject"].Aliases.Contains("SnapshotMustNotChange"))
            throw new InvalidOperationException("Snapshot retained live aliases.");
        var dictionary = (IDictionary<string, CommandParameterSnapshot>)snapshot;
        try { dictionary.Clear(); }
        catch (NotSupportedException) { return; }
        throw new InvalidOperationException("Snapshot can be modified.");
    }

    public static void Configure(IDisposable cache, string script)
    {
        var runspace = (Runspace)CacheType.GetField("_runspace", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(cache);
        using (var ps = PowerShell.Create())
        {
            ps.Runspace = runspace;
            ps.AddScript(script);
            ps.Invoke();
            if (ps.HadErrors) throw new InvalidOperationException(ps.Streams.Error[0].ToString());
        }
    }
}
'@
    }

    BeforeEach {
        $cache = [MetadataSnapshotTests]::Create()
        $lookupCache = $cache.GetType().GetField(
            '_commandInfoCache', [System.Reflection.BindingFlags]'NonPublic, Instance').GetValue($cache)
    }

    AfterEach {
        $cache.Dispose()
    }

    It "publishes one immutable static snapshot to concurrent readers" {
        [MetadataSnapshotTests]::ConcurrentStatic($cache)
        $lookupCache.Count | Should -Be 1
    }

    It "detaches parameter facts from the live command" {
        [MetadataSnapshotTests]::CheckDetached($cache)
    }

    It "does not cache provider dynamic metadata or aliases" {
        foreach ($name in 'Get-Item', 'echo') {
            $first = [MetadataSnapshotTests]::Parameters($cache, $name)
            $second = [MetadataSnapshotTests]::Parameters($cache, $name)
            [object]::ReferenceEquals($first, $second) | Should -BeFalse
        }
    }

    It "does not cache function metadata" {
        [MetadataSnapshotTests]::Configure($cache, 'function global:Test-SnapshotFunction { param($Example) }')
        $first = [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotFunction')
        $second = [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotFunction')
        $first.ContainsKey('Example') | Should -BeTrue
        [object]::ReferenceEquals($first, $second) | Should -BeFalse
    }

    It "observes changed dynamic parameters while retaining the previous detached result" {
        [MetadataSnapshotTests]::Configure($cache, 'Import-Module -Assembly ([SnapshotDynamicCommand].Assembly)')
        [SnapshotDynamicCommand]::ParameterName = 'First'
        $first = [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotDynamic')
        [SnapshotDynamicCommand]::ParameterName = 'Second'
        $second = [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotDynamic')
        $first.ContainsKey('First') | Should -BeTrue
        $first.ContainsKey('Second') | Should -BeFalse
        $second.ContainsKey('Second') | Should -BeTrue
        $second.ContainsKey('First') | Should -BeFalse
    }

    It "bypasses static snapshots without replacing the cached snapshot" {
        $first = [MetadataSnapshotTests]::Parameters($cache, 'Write-Output')
        $fresh = [MetadataSnapshotTests]::Parameters($cache, 'Write-Output', $true)
        [object]::ReferenceEquals($first, $fresh) | Should -BeFalse
        [object]::ReferenceEquals($first, [MetadataSnapshotTests]::Parameters($cache, 'write-output')) | Should -BeTrue
    }

    It "combines mandatory metadata under one query and caches only static summaries" {
        $first = [MetadataSnapshotTests]::Mandatory($cache, 'Write-Warning')
        $first | Should -Contain 'Message'
        [object]::ReferenceEquals($first, [MetadataSnapshotTests]::Mandatory($cache, 'Write-Warning')) | Should -BeTrue
        $first = [MetadataSnapshotTests]::Mandatory($cache, 'Get-Item')
        [object]::ReferenceEquals($first, [MetadataSnapshotTests]::Mandatory($cache, 'Get-Item')) | Should -BeFalse
    }

    It "does not return cached snapshots after disposal" {
        $null = [MetadataSnapshotTests]::Parameters($cache, 'Write-Output')
        $cache.Dispose()
        [MetadataSnapshotTests]::Parameters($cache, 'Write-Output') | Should -BeNullOrEmpty
        [MetadataSnapshotTests]::Mandatory($cache, 'Write-Output') | Should -BeNullOrEmpty
    }

    It "returns null when the engine cannot resolve Get-Command itself" {
        [MetadataSnapshotTests]::Configure($cache, @'
New-Module -Name Microsoft.PowerShell.Core -ScriptBlock {
    function Get-Command {
        [CmdletBinding()]
        param($Name)
        Write-Error -Exception ([System.Management.Automation.CommandNotFoundException]::new(
            'Injected command resolution failure')) -ErrorAction Continue
    }
    Export-ModuleMember -Function Get-Command
} | Import-Module -Force
'@)
        $lookup = $cache.GetType().GetMethod('GetCommandInfo')
        $lookup.Invoke($cache, @('Write-Output', $null, $false)) | Should -BeNullOrEmpty
    }

    It "does not cache a command resolution failure as a missing command" {
        [MetadataSnapshotTests]::Configure($cache, @'
New-Module -Name Microsoft.PowerShell.Core -ScriptBlock {
    function Get-Command {
        param($Name)
        Write-Error -Exception ([System.Management.Automation.CommandNotFoundException]::new(
            'Transient command resolution failure')) -ErrorAction Continue
    }
    Export-ModuleMember -Function Get-Command
} | Import-Module -Force
'@)
        [MetadataSnapshotTests]::Parameters($cache, 'Write-Output') | Should -BeNullOrEmpty
        [MetadataSnapshotTests]::Configure($cache, 'Remove-Module Microsoft.PowerShell.Core')
        [MetadataSnapshotTests]::Parameters($cache, 'Write-Output').ContainsKey('InputObject') | Should -BeTrue
    }

    It "retains negative caching for genuine missing commands" {
        1..2 | ForEach-Object {
            [MetadataSnapshotTests]::Parameters($cache, 'Test-NonexistentMetadataCommand') | Should -BeNullOrEmpty
        }
        $lookupCache.Count | Should -Be 1
    }

    It "resolves module-qualified command names against the named module only" {
        $lookup = $cache.GetType().GetMethod('GetCommandInfo')
        $lookup.Invoke($cache, @('Microsoft.PowerShell.Utility\Write-Output', $null, $false)).Name |
            Should -BeExactly 'Write-Output'
        $lookup.Invoke($cache, @('Microsoft.PowerShell.Management\Write-Output', $null, $false)) |
            Should -BeNullOrEmpty
    }

    It "handles invalid PowerShell metadata centrally for <Method>" -TestCases @(
        @{ Method = 'GetCommandParameters' }
        @{ Method = 'GetCommandParameterSets' }
        @{ Method = 'GetParameterSnapshot' }
        @{ Method = 'GetMandatoryParameterNames' }
    ) {
        param($Method)
        [MetadataSnapshotTests]::Configure($cache, @'
function global:Test-InvalidMetadata {
    param([Parameter(ParameterSetName='A')][Parameter(ParameterSetName='A')]$Example)
}
'@)
        [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-InvalidMetadata', $Method) | Should -BeNullOrEmpty

        [MetadataSnapshotTests]::Configure($cache, 'function global:Test-InvalidMetadata { param([Parameter(Mandatory)]$Example) }')
        [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-InvalidMetadata', $Method) | Should -Not -BeNullOrEmpty
    }

    It "returns unavailable native-command metadata without an exception" {
        $name = if ([Environment]::OSVersion.Platform -eq 'Win32NT') { 'cmd.exe' } else { 'sh' }
        [MetadataSnapshotTests]::Parameters($cache, $name) | Should -BeNullOrEmpty
        [MetadataSnapshotTests]::Mandatory($cache, $name) | Should -BeNullOrEmpty
    }

    Context "Runspace affinity failures" {
        BeforeEach {
            [MetadataSnapshotTests]::Configure($cache, @'
New-Module -Name Microsoft.PowerShell.Core -ScriptBlock {
    function Get-Command { param($Name); [FaultingMetadataInfo]::new() }
    Export-ModuleMember -Function Get-Command
} | Import-Module -Force
'@)
            [FaultingMetadataInfo]::Failure = [InvalidOperationException]::new('Injected metadata failure')
            [FaultingMetadataInfo]::FailuresRemaining = 1
        }

        It "returns null for <Method> and recovers on the next call" -TestCases @(
            @{ Method = 'GetCommandParameters' }
            @{ Method = 'GetParameterSnapshot' }
            @{ Method = 'GetMandatoryParameterNames' }
        ) {
            param($Method)
            [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-SnapshotStatic', $Method) | Should -BeNullOrEmpty
            [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-SnapshotStatic', $Method) | Should -Not -BeNullOrEmpty
        }

        It "does not cache a failed metadata result" {
            [FaultingMetadataInfo]::Failure = [NullReferenceException]::new('Injected metadata failure')
            [FaultingMetadataInfo]::FailuresRemaining = 2
            [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') | Should -BeNullOrEmpty
            [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') | Should -BeNullOrEmpty
            [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic').ContainsKey('Example') | Should -BeTrue
        }

        It "returns null for an explicitly fresh lookup that fails" {
            [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic', $true) | Should -BeNullOrEmpty
        }

        It "does not swallow unrelated exceptions" {
            [FaultingMetadataInfo]::Failure = [ArgumentException]::new('Unexpected metadata failure')
            { [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') } |
                Should -Throw '*Unexpected metadata failure*'
        }

        It "contains metadata operations unsupported by PowerShell" {
            [FaultingMetadataInfo]::Failure = [System.Management.Automation.PSNotSupportedException]::new(
                'Unsupported metadata operation')
            [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') | Should -BeNullOrEmpty
        }
    }
}
