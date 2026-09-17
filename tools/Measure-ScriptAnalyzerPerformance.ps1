# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ModulePath,

    [Parameter(Mandatory)]
    [string]$ScriptPath,

    [Parameter(Mandatory)]
    [string]$ResultPath,

    [switch]$Recurse,

    [string]$SettingsPath,

    # Analyses to run, and discard, between the cold and warm measurements. Tiered compilation
    # promotes hot methods over the first few iterations and there is no way to wait for that
    # queue to drain, so the ramp has to be spent before the warm run rather than inside it.
    [int]$WarmupCount = 4
)

$ErrorActionPreference = 'Stop'
$ModulePath = (Resolve-Path -LiteralPath $ModulePath).Path
$ScriptPath = (Resolve-Path -LiteralPath $ScriptPath).Path
$analyzerArguments = @{
    Path = $ScriptPath
    Recurse = $Recurse
    ErrorAction = 'Stop'
}
if ($SettingsPath) {
    $SettingsPath = (Resolve-Path -LiteralPath $SettingsPath).Path
    $analyzerArguments.Settings = $SettingsPath
}

# Run this script in a new -NoProfile shell for each build. Import and shell
# startup are excluded; cold means the first analysis in this process.
if (Get-Module PSScriptAnalyzer) {
    throw 'PSScriptAnalyzer is already loaded; run this benchmark in a fresh shell.'
}
$module = Import-Module -Name $ModulePath -PassThru
$expectedModuleBase = Split-Path -Parent $ModulePath
$expectedAssemblyPath = (Resolve-Path -LiteralPath (
    Join-Path $expectedModuleBase "PSv$($PSVersionTable.PSVersion.Major)/Microsoft.Windows.PowerShell.ScriptAnalyzer.dll"
)).Path
$command = Get-Command PSScriptAnalyzer\Invoke-ScriptAnalyzer -CommandType Cmdlet
if ($module.Name -ne 'PSScriptAnalyzer' -or
    $module.ModuleBase -ne $expectedModuleBase -or
    $command.Module.ModuleBase -ne $expectedModuleBase -or
    $command.ImplementingType.Assembly.Location -ne $expectedAssemblyPath) {
    throw "The loaded module or Invoke-ScriptAnalyzer assembly does not match the requested build at '$ModulePath'."
}
$inputItem = Get-Item -LiteralPath $ScriptPath
$inputRoot = if ($inputItem.PSIsContainer) { $ScriptPath } else { Split-Path -Parent $ScriptPath }

function ConvertTo-WorkloadPath([string]$Path) {
    if ([string]::IsNullOrEmpty($Path)) { return $Path }
    if ([IO.Path]::IsPathRooted($Path)) {
        return [IO.Path]::GetRelativePath($inputRoot, $Path).Replace('\', '/')
    }
    return $Path.Replace('\', '/')
}

function ConvertTo-DiagnosticJson($Diagnostic) {
    $corrections = @(foreach ($correction in $Diagnostic.SuggestedCorrections) {
        [ordered]@{
            File = ConvertTo-WorkloadPath $correction.File
            StartLine = $correction.StartLineNumber
            StartColumn = $correction.StartColumnNumber
            EndLine = $correction.EndLineNumber
            EndColumn = $correction.EndColumnNumber
            Text = $correction.Text
            Description = $correction.Description
        }
    })
    [ordered]@{
        RuleName = $Diagnostic.RuleName
        Severity = $Diagnostic.Severity.ToString()
        Message = $Diagnostic.Message.Replace($inputRoot, '<workload>')
        ScriptName = $Diagnostic.ScriptName
        ScriptPath = ConvertTo-WorkloadPath $Diagnostic.ScriptPath
        RuleSuppressionID = $Diagnostic.RuleSuppressionID
        IsSuppressed = $Diagnostic.IsSuppressed
        Extent = if ($null -ne $Diagnostic.Extent) {
            [ordered]@{
                File = ConvertTo-WorkloadPath $Diagnostic.Extent.File
                StartLine = $Diagnostic.Extent.StartLineNumber
                StartColumn = $Diagnostic.Extent.StartColumnNumber
                EndLine = $Diagnostic.Extent.EndLineNumber
                EndColumn = $Diagnostic.Extent.EndColumnNumber
                StartOffset = $Diagnostic.Extent.StartOffset
                EndOffset = $Diagnostic.Extent.EndOffset
                Text = $Diagnostic.Extent.Text
            }
        } else { $null }
        SuggestedCorrections = $corrections
    } | ConvertTo-Json -Depth 10 -Compress
}

if ($inputItem.PSIsContainer) {
    $inputFiles = @(Get-ChildItem -LiteralPath $ScriptPath -File -Recurse:$Recurse |
        Where-Object Extension -In '.ps1', '.psm1', '.psd1' |
        Sort-Object FullName)
    if ($inputFiles.Count -eq 0) {
        throw "No PowerShell files found at '$ScriptPath'."
    }
    $fileHashes = foreach ($file in $inputFiles) {
        $relativePath = [IO.Path]::GetRelativePath($ScriptPath, $file.FullName).Replace('\', '/')
        "$relativePath`:$((Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash)"
    }
    $inputHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
        [Text.Encoding]::UTF8.GetBytes(($fileHashes -join "`n"))))
} else {
    $inputFiles = @($inputItem)
    $inputHash = (Get-FileHash -LiteralPath $ScriptPath -Algorithm SHA256).Hash
}
$results = [ordered]@{
    PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    ModuleVersion = $module.Version.ToString()
    ModuleManifest = $ModulePath
    ModulePath = $module.Path
    AnalyzerAssemblyPath = $command.ImplementingType.Assembly.Location
    ScriptPath = $ScriptPath
    InputSHA256 = $inputHash
    InputFileCount = $inputFiles.Count
    Recurse = [bool]$Recurse
    SettingsPath = $SettingsPath
    SettingsSHA256 = if ($SettingsPath) { (Get-FileHash -LiteralPath $SettingsPath -Algorithm SHA256).Hash } else { $null }
    Culture = [Globalization.CultureInfo]::CurrentCulture.Name
    UICulture = [Globalization.CultureInfo]::CurrentUICulture.Name
    StopwatchFrequency = [System.Diagnostics.Stopwatch]::Frequency
    WarmupCount = $WarmupCount
}

foreach ($run in 'Cold', 'Warm') {
    if ($run -eq 'Warm') {
        for ($warmup = 0; $warmup -lt $WarmupCount; $warmup++) {
            $null = & $command @analyzerArguments
        }
    }

    # Settle the allocations from module load and from canonicalizing the previous run, so that
    # collecting them is not charged to whichever measurement happens to trigger it.
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    [System.GC]::Collect()

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $diagnostics = @(& $command @analyzerArguments)
    $stopwatch.Stop()
    $results["${run}Seconds"] = $stopwatch.Elapsed.TotalSeconds
    $results["${run}DiagnosticCount"] = $diagnostics.Count
    # Canonicalize outside the timed region, retaining duplicates but ignoring
    # nondeterministic rule completion order and checkout-root differences.
    [string[]]$diagnosticJson = @($diagnostics | ForEach-Object { ConvertTo-DiagnosticJson $_ })
    [Array]::Sort($diagnosticJson, [StringComparer]::Ordinal)
    $results["${run}DiagnosticsSHA256"] = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData(
        [Text.Encoding]::UTF8.GetBytes(($diagnosticJson -join "`n"))))
    $results["${run}Diagnostics"] = @($diagnosticJson | ForEach-Object { ConvertFrom-Json -InputObject $_ })
}

[pscustomobject]$results | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $ResultPath -Encoding utf8
