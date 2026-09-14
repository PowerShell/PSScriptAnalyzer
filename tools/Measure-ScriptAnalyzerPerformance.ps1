# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ModulePath,

    [Parameter(Mandatory)]
    [string]$ScriptPath,

    [Parameter(Mandatory)]
    [string]$ResultPath
)

$ErrorActionPreference = 'Stop'
$ModulePath = (Resolve-Path -LiteralPath $ModulePath).Path
$ScriptPath = (Resolve-Path -LiteralPath $ScriptPath).Path

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
$results = [ordered]@{
    PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    ModuleVersion = $module.Version.ToString()
    ModuleManifest = $ModulePath
    ModulePath = $module.Path
    AnalyzerAssemblyPath = $command.ImplementingType.Assembly.Location
    ScriptPath = $ScriptPath
    ScriptSHA256 = (Get-FileHash -LiteralPath $ScriptPath -Algorithm SHA256).Hash
}

foreach ($run in 'Cold', 'Warm') {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $diagnostics = @(& $command -Path $ScriptPath -ErrorAction Stop)
    $stopwatch.Stop()
    $results["${run}Seconds"] = $stopwatch.Elapsed.TotalSeconds
    $results["${run}DiagnosticCount"] = $diagnostics.Count
}

[pscustomobject]$results | ConvertTo-Json | Set-Content -LiteralPath $ResultPath -Encoding utf8
