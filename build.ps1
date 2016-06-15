[CmdletBinding()]
param(
    [ValidateSet('PSV3 Debug','PSV3 Release','Debug','Release')]
    [string] $Configuration = 'Debug',

    [switch] $BuildSolution = $false,

    [switch] $CleanSolution = $false,

    [switch] $BuildDocs = $false,

    [switch] $CleanOutput = $false,

    [switch] $Install = $false
)

# Some cmdlets like copy-item do not respond to the $verbosepreference variable
# hence we set it explicitly
$verbosity = $false
if ($VerbosePreference.Equals([System.Management.Automation.ActionPreference]'Continue'))
{
    $verbosity = $true
}

Function CreateIfNotExists([string] $folderPath)
{
    if (-not (Test-Path $folderPath))
    {
        New-Item -Path $folderPath -ItemType Directory -Verbose:$verbosity
    }
}

$projectRoot = Resolve-path .
$solutionPath = Join-Path $projectRoot 'PSScriptAnalyzer.sln'
$outPath = Join-Path $projectRoot 'out'
$destinationPath = Join-Path $outPath PSScriptAnalyzer

if (-not (Test-Path $solutionPath))
{
    throw "not the right directory"
}

$buildCmd = Join-Path $projectRoot "build.cmd"
if (-not (Test-Path $buildCmd))
{
    throw "cannot find build.cmd"
}

if ($CleanOutput)
{
    Remove-Item -Recurse $outPath\* -Force -Verbose:$verbosity
}

if ($CleanSolution)
{
    & $buildCmd $solutionPath $Configuration 'clean'
}

if ($BuildSolution)
{
    & $buildCmd $solutionPath $Configuration
}

if ($BuildDocs)
{
    $docsPath = Join-Path $projectRoot 'docs'
    $markdownDocsPath = Join-Path $docsPath 'markdown'
    $outputDocsPath = Join-Path $destinationPath en-US

    CreateIfNotExists($outputDocsPath)
    # copy the about help file
    Copy-Item -Path $docsPath\about_PSScriptAnalyzer.help.txt -Destination $outputDocsPath -Force -Verbose:$verbosity

    # Build documentation using platyPS
    if ((Get-Module PlatyPS -ListAvailable -Verbose:$verbosity) -eq $null)
    {
        throw "Cannot find PlatyPS. Please install it from https://www.powershellgallery.com."
    }
    if ((Get-Module PlatyPS -Verbose:$verbosity) -eq $null)
    {
        Import-Module PlatyPS -Verbose:$verbosity
    }
    if (-not (Test-Path $markdownDocsPath -Verbose:$verbosity))
    {
        throw "Cannot find markdown documentation folder."
    }
    New-ExternalHelp -Path $markdownDocsPath -OutputPath $outputDocsPath -Force -Verbose:$verbosity
}

if ($Install)
{
    $modulePath = Join-Path (Split-Path $profile) 'Modules'
    if (-not (Test-Path $modulePath))
    {
        New-Item -Path $modulePath -ItemType Directory -Force -Verbose:$verbosity
    }
    if (-not (Test-Path -Path $destinationPath))
    {
        throw "Please build the module first."
    }
    Copy-Item -Path $destinationPath -Destination $modulePath -Recurse -Verbose:$verbosity
}
