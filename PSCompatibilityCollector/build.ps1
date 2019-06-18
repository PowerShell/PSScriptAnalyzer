# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[CmdletBinding(DefaultParameterSetName='AllFrameworks')]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    $Configuration = 'Debug',

    [Parameter()]
    [ValidateSet('netstandard2.0', 'net452')]
    [string]
    $Framework,

    [switch]
    $Test,

    [switch]
    $Clean
)

$ErrorActionPreference = 'Stop'

if ($IsWindows -eq $false) {
    $script:TargetFrameworks = 'netstandard2.0'
} else {
    $script:TargetFrameworks = 'netstandard2.0','net452'
}

$script:ModuleName = Split-Path $PSScriptRoot -Leaf

$script:Psm1Path = [System.IO.Path]::Combine($PSScriptRoot, "$script:ModuleName.psm1")
$script:Psd1Path = [System.IO.Path]::Combine($PSScriptRoot, "$script:ModuleName.psd1")

$script:BinModDir = [System.IO.Path]::Combine($PSScriptRoot, 'out', "$script:ModuleName")
$script:BinModSrcDir = Join-Path $PSScriptRoot 'Microsoft.PowerShell.CrossCompatibility'

$script:PublishDlls = @{
    'net452' = @('Microsoft.PowerShell.CrossCompatibility.dll', 'Microsoft.PowerShell.CrossCompatibility.pdb', 'Newtonsoft.Json.dll')
    'netstandard2.0' = @('Microsoft.PowerShell.CrossCompatibility.dll', 'Microsoft.PowerShell.CrossCompatibility.pdb', 'Newtonsoft.Json.dll')
}

function Invoke-CrossCompatibilityModuleBuild
{
    param(
        [Parameter()]
        [ValidateSet('netstandard2.0', 'net452')]
        [string]
        $Framework = 'netstandard2.0',

        [Parameter()]
        [ValidateSet('Debug', 'Release')]
        [string]
        $Configuration = 'Debug'
    )

    Push-Location $script:BinModSrcDir
    try
    {
        dotnet publish -f $Framework -c $Configuration
    }
    finally
    {
        Pop-Location
    }
}

function Publish-CrossCompatibilityModule
{
    param(
        [Parameter()]
        [string]
        $SrcRootDir = $script:BinModSrcDir,

        [Parameter()]
        [string]
        $DestinationDir = $script:BinModDir,

        [Parameter()]
        [string[]]
        $TargetFramework = $script:TargetFrameworks
    )

    if (-not (Test-Path $DestinationDir))
    {
        New-Item -ItemType Directory $DestinationDir
    }
    elseif (-not (Test-Path $DestinationDir -PathType Container))
    {
        throw "$DestinationDir exists but is not a directory. Aborting."
    }

    Copy-Item -LiteralPath $script:Psd1Path -Destination (Join-Path $DestinationDir "$script:ModuleName.psd1")
    Copy-Item -LiteralPath $script:Psm1Path -Destination (Join-Path $DestinationDir "$script:ModuleName.psm1")

    foreach ($framework in $TargetFramework)
    {
        $dest = Join-Path $DestinationDir $framework
        if (-not (Test-Path $dest))
        {
            $null = New-Item -ItemType Directory -Path $dest
        }

        $binPath = [System.IO.Path]::Combine($SrcRootDir, 'bin', $Configuration, $framework, 'publish')
        $dlls = $script:PublishDlls[$framework]

        foreach ($dll in $dlls)
        {
            $dllPath = Join-Path $binPath $dll
            $null = Copy-Item -LiteralPath $dllPath -Destination $dest
        }
    }
}

if ($Clean)
{
    Remove-Item -Force -Recurse $script:BinModDir -ErrorAction Ignore

    if (Test-Path $script:BinModDir)
    {
        throw "'$script:BinModDir' still present, aborting build"
    }
}

# Only build if the output directory does not exist
if (-not (Test-Path "$PSScriptRoot/out/$script:ModuleName"))
{
    if ($Framework)
    {
        Invoke-CrossCompatibilityModuleBuild -Configuration $Configuration -Framework $Framework
        Publish-CrossCompatibilityModule -TargetFramework $Framework
    }
    else
    {
        foreach ($f in $script:TargetFrameworks)
        {
            Invoke-CrossCompatibilityModuleBuild -Framework $f -Configuration $Configuration
        }
        Publish-CrossCompatibilityModule
    }
}
else
{
    Write-Verbose "$script:ModuleName module already built -- skipping build"
    Write-Verbose "Use '-Clean' to force building"
}

if ($Test)
{
    $testPath = "$PSScriptRoot/Tests"
    & (Get-Process -Id $PID).Path -Command "Invoke-Pester -Path '$testPath'"
}
