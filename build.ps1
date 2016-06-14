[CmdletBinding()]
param(
    [Parameter(ParameterSetName='Build')]
    [ValidateSet('PSV3 Debug','PSV3 Release','Debug','Release')]
    [string] $Configuration = 'Debug',

    [Parameter(ParameterSetName='Build')]
    [switch] $CleanSolution = $false,    

    [Parameter(ParameterSetName='Documentation')]
    [switch] $Documentation = $false,

    [Parameter(ParameterSetName='Clean')]    
    [switch] $Clean = $false
)

Function CreateIfNotExists([string] $folderPath)
{
    if (-not (Test-Path $folderPath))
    {
        New-Item -Path $folderPath -ItemType Directory
    }
}

$projectRoot = Resolve-path .
$solutionPath = Join-Path $projectRoot 'PSScriptAnalyzer.sln'
$outPath = Join-Path $projectRoot 'out'
$enginePath = Join-Path $projectRoot "Engine"
$engineBinariesPath = Join-Path $enginePath "bin\$Configuration"
$rulesBinariesPath = Join-Path $projectRoot "Rules\bin\$Configuration"
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


if ($PSCmdlet.ParameterSetName -eq 'Clean')
{
    Remove-Item -Recurse $outPath\* -Force
}

if ($PSCmdlet.ParameterSetName -eq 'Build')
{
    if ($CleanSolution)
    {
        & $buildCmd $solutionPath $Configuration 'clean'
        return    
    }
    & $buildCmd $solutionPath $Configuration    
    
    CreateIfNotExists($destinationPath)
    CreateIfNotExists("$destinationPath\Settings")

    Copy-Item -Path $enginePath\*.ps1xml -Destination $destinationPath\. -Force
    Copy-Item -Path "$enginePath\PSScriptAnalyzer.psm1" -Destination $destinationPath\. -Force
    Copy-Item -Path "$enginePath\PSScriptAnalyzer.psd1" -Destination $destinationPath\. -Force
    Copy-Item -Path "$enginePath\Settings" -Destination $destinationPath\. -Force -Recurse

    if ($Configuration -match 'PSV3')
    {
        $destinationPath = Join-Path $destinationPath "PSv3"
        CreateIfNotExists($destinationPath)
    }
    Copy-Item -Path $engineBinariesPath\*.dll -Destination $destinationPath\. -Force
    Copy-Item -Path $rulesBinariesPath\*.dll -Destination $destinationPath\. -Force
}

if ($PSCmdlet.ParameterSetName -eq 'Documentation')
{
    CreateIfNotExists("$destinationPath\en-US\")
    Copy-Item -Path "$enginePath\about_PSScriptAnalyzer.help.txt" -Destination $destinationPath\en-US\. -Force
    
    # Build documentation using platyPS
    if ((Get-Module PlatyPS -ListAvailable) -eq $null)
    {
        throw "Cannot find PlatyPS. Please install it from https://www.powershellgallery.com."
    }
    Import-Module PlatyPS -Force
    $docsPath = Join-Path $projectRoot docs
    if (-not (Test-Path $docsPath))
    {
        throw "Cannot find markdown documentation folder."
    }
    New-ExternalHelp -Path $projectRoot\docs -OutputPath $destinationPath\en-US -Force
}
