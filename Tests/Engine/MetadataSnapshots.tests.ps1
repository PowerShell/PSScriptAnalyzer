# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Detached command metadata" {
    BeforeAll {
        $null = Invoke-ScriptAnalyzer -ScriptDefinition 'Write-Output example'
        $assembly = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper].Assembly
        $telemetry = $assembly.GetType('Microsoft.Windows.PowerShell.ScriptAnalyzer.PerformanceTelemetry')
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

    public static IDisposable Create() => (IDisposable)Activator.CreateInstance(CacheType);

    public static IReadOnlyDictionary<string, CommandParameterSnapshot> Parameters(
        IDisposable cache, string name, bool bypass = false)
    {
        return (IReadOnlyDictionary<string, CommandParameterSnapshot>)CacheType.GetMethod("GetParameterSnapshot")
            .Invoke(cache, new object[] { name, null, bypass });
    }

    public static object Mandatory(IDisposable cache, string name)
        => CacheType.GetMethod("GetMandatoryParameterNames").Invoke(cache, new object[] { name });

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
        $telemetry.GetMethod('Reset').Invoke($null, @())
        $telemetry.GetProperty('Enabled').SetValue($null, $true)
        $retriesEnabled = $null -ne $cache.GetType().GetField(
            'MaxLookupAttempts', [System.Reflection.BindingFlags]'NonPublic, Static')
    }

    AfterEach {
        $telemetry.GetProperty('Enabled').SetValue($null, $false)
        $cache.Dispose()
    }

    It "publishes one immutable static snapshot to concurrent readers" {
        [MetadataSnapshotTests]::ConcurrentStatic($cache)
        $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
        $stats['LookupMisses'] | Should -Be 1
        $stats['MetadataQueries'] | Should -Be 1
        $stats['LockHoldTicks'] | Should -BeGreaterThan 0
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
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataQueries'] | Should -Be 4
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
        $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
        $stats['LookupBypasses'] | Should -Be 1
        $stats['MetadataQueries'] | Should -Be 2
    }

    It "combines mandatory metadata under one query and caches only static summaries" {
        $first = [MetadataSnapshotTests]::Mandatory($cache, 'Write-Warning')
        $first | Should -Contain 'Message'
        [object]::ReferenceEquals($first, [MetadataSnapshotTests]::Mandatory($cache, 'Write-Warning')) | Should -BeTrue
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataQueries'] | Should -Be 1
        $first = [MetadataSnapshotTests]::Mandatory($cache, 'Get-Item')
        [object]::ReferenceEquals($first, [MetadataSnapshotTests]::Mandatory($cache, 'Get-Item')) | Should -BeFalse
    }

    It "does not return cached snapshots after disposal" {
        $null = [MetadataSnapshotTests]::Parameters($cache, 'Write-Output')
        $cache.Dispose()
        [MetadataSnapshotTests]::Parameters($cache, 'Write-Output') | Should -BeNullOrEmpty
        [MetadataSnapshotTests]::Mandatory($cache, 'Write-Output') | Should -BeNullOrEmpty
    }

    It "counts injected resolution failures and honors the compiled retry mode" {
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
        $cacheType = $cache.GetType()
        $lookup = $cacheType.GetMethod('GetCommandInfo')
        $retryLimit = $cacheType.GetField('MaxLookupAttempts', [System.Reflection.BindingFlags]'NonPublic, Static')
        if ($null -eq $retryLimit) {
            { $lookup.Invoke($cache, @('Write-Output', $null, $false)) } |
                Should -Throw '*Injected command resolution failure*'
            $expectedFailures = 1
            $expectedRetries = 0
        }
        else {
            $lookup.Invoke($cache, @('Write-Output', $null, $false)) | Should -BeNullOrEmpty
            $expectedFailures = $retryLimit.GetRawConstantValue()
            $expectedRetries = $expectedFailures - 1
        }
        $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
        $stats['LookupResolutionFailures'] | Should -Be $expectedFailures
        $stats['LookupRetries'] | Should -Be $expectedRetries
    }

    It "does not retain a null command lookup" {
        [MetadataSnapshotTests]::Parameters($cache, 'Test-LaterMetadata') | Should -BeNullOrEmpty
        [MetadataSnapshotTests]::Configure($cache, 'function global:Test-LaterMetadata { param($Example) }')
        [MetadataSnapshotTests]::Parameters($cache, 'Test-LaterMetadata').ContainsKey('Example') | Should -BeTrue
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['LookupMisses'] | Should -Be 2
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
        if ($retriesEnabled) {
            [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-InvalidMetadata', $Method) | Should -BeNullOrEmpty
        }
        else {
            { [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-InvalidMetadata', $Method) } | Should -Throw
        }
        $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
        $stats['MetadataFailures'] | Should -Be 1
        $stats['MetadataRetries'] | Should -Be 0

        [MetadataSnapshotTests]::Configure($cache, 'function global:Test-InvalidMetadata { param([Parameter(Mandatory)]$Example) }')
        [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-InvalidMetadata', $Method) | Should -Not -BeNullOrEmpty
    }

    It "returns unavailable native-command metadata without an exception or retry" {
        $name = if ([Environment]::OSVersion.Platform -eq 'Win32NT') { 'cmd.exe' } else { 'sh' }
        [MetadataSnapshotTests]::Parameters($cache, $name) | Should -BeNullOrEmpty
        [MetadataSnapshotTests]::Mandatory($cache, $name) | Should -BeNullOrEmpty
        $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
        $stats['MetadataFailures'] | Should -Be 0
        $stats['MetadataRetries'] | Should -Be 0
    }

    Context "Runspace affinity recovery" {
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

        It "retries <Method> with fresh metadata only in the retry-enabled build" -TestCases @(
            @{ Method = 'GetCommandParameters' }
            @{ Method = 'GetParameterSnapshot' }
            @{ Method = 'GetMandatoryParameterNames' }
        ) {
            param($Method)
            if ($retriesEnabled) {
                [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-SnapshotStatic', $Method) | Should -Not -BeNullOrEmpty
            }
            else {
                { [MetadataSnapshotTests]::ReadMetadata($cache, 'Test-SnapshotStatic', $Method) } |
                    Should -Throw '*Injected metadata failure*'
            }
            $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
            $stats['MetadataFailures'] | Should -Be 1
            $stats['MetadataRetries'] | Should -Be ([int]$retriesEnabled)
            $stats['LookupBypasses'] | Should -Be ([int]$retriesEnabled)
        }

        It "bounds recovery and does not cache a failed metadata result" {
            [FaultingMetadataInfo]::Failure = [NullReferenceException]::new('Injected metadata failure')
            [FaultingMetadataInfo]::FailuresRemaining = 2
            if ($retriesEnabled) {
                [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') | Should -BeNullOrEmpty
            }
            else {
                { [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') } |
                    Should -Throw '*Injected metadata failure*'
            }
            $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
            $stats['MetadataFailures'] | Should -Be (1 + [int]$retriesEnabled)
            $stats['MetadataRetries'] | Should -Be ([int]$retriesEnabled)
            [FaultingMetadataInfo]::FailuresRemaining = 0
            [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic').ContainsKey('Example') | Should -BeTrue
            $telemetry.GetMethod('Snapshot').Invoke($null, @())['LookupMisses'] | Should -Be 2
        }

        It "does not retry an explicitly fresh lookup" {
            if ($retriesEnabled) {
                [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic', $true) | Should -BeNullOrEmpty
            }
            else {
                { [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic', $true) } | Should -Throw
            }
            $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataRetries'] | Should -Be 0
        }

        It "does not swallow unrelated exceptions" {
            [FaultingMetadataInfo]::Failure = [ArgumentException]::new('Unexpected metadata failure')
            { [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') } |
                Should -Throw '*Unexpected metadata failure*'
            $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataFailures'] | Should -Be 0
        }

        It "does not retry metadata operations unsupported by PowerShell" {
            [FaultingMetadataInfo]::Failure = [System.Management.Automation.PSNotSupportedException]::new(
                'Unsupported metadata operation')
            if ($retriesEnabled) {
                [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') | Should -BeNullOrEmpty
            }
            else {
                { [MetadataSnapshotTests]::Parameters($cache, 'Test-SnapshotStatic') } |
                    Should -Throw '*Unsupported metadata operation*'
            }
            $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
            $stats['MetadataFailures'] | Should -Be 1
            $stats['MetadataRetries'] | Should -Be 0
        }
    }
}
