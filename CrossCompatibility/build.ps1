[CmdletBinding(DefaultParameterSetName='AllFrameworks')]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    $Configuration = 'Debug',

    [Parameter()]
    [ValidateSet('netstandard2.0', 'net451')]
    [string]
    $Framework
)

$ErrorActionPreference = 'Stop'

$script:TargetFrameworks = 'netstandard2.0','net451'
$script:BinModDir = Join-Path $PSScriptRoot 'CrossCompatibilityBinary'
$script:BinModSrcDir = Join-Path $PSScriptRoot 'CrossCompatibility'

$script:PublishDlls = @{
    'net451' = @('CrossCompatibility.dll', 'CrossCompatibility.pdb', 'Newtonsoft.Json.dll')
    'netstandard2.0' = @('CrossCompatibility.dll', 'CrossCompatibility.pdb', 'Newtonsoft.Json.dll')
}

function Invoke-BinaryModuleBuild
{
    param(
        [Parameter()]
        [ValidateSet('netstandard2.0', 'net451')]
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

function Restore-BinaryModule
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

if (-not $Framework)
{
    foreach ($f in $script:TargetFrameworks)
    {
        Invoke-BinaryModuleBuild -Framework $f -Configuration $Configuration
    }
    Restore-BinaryModule
    return
}

Invoke-BinaryModuleBuild @PSBoundParameters
Restore-BinaryModule -TargetFramework $Framework