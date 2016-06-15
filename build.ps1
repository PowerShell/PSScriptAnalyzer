[CmdletBinding()]
param(
   
    [ValidateSet('PSV3 Debug','PSV3 Release','Debug','Release')]
    [string] $Configuration = 'Debug',

    [switch] $BuildSolution = $false,

    [switch] $CleanSolution = $false,

    [switch] $BuildDocs = $false,

    [switch] $CleanOutput = $false,

    [switch] $Install = $false,

    [switch] $Uninstall = $false,

    [switch] $Test = $false,

    [switch] $Engine = $false,

    [switch] $Rules = $false
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

$projectRoot = Resolve-path (Split-Path $MyInvocation.InvocationName)
$solutionPath = Join-Path $projectRoot 'PSScriptAnalyzer.sln'
$outPath = Join-Path $projectRoot 'out'
$destinationPath = Join-Path $outPath PSScriptAnalyzer

if (-not (Test-Path $solutionPath))
{
    $errMsg = "{0} not the right directory" -f $solutionPath
    throw $errMsg
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


$moduleRootPath = Join-Path (Split-Path $profile) 'Modules'
$modulePSSAPath = Join-Path $moduleRootPath 'PSScriptAnalyzer'
if ($Install)
{
    if (-not (Test-Path $moduleRootPath))
    {
        New-Item -Path $moduleRootPath -ItemType Directory -Force -Verbose:$verbosity
    }
    if (-not (Test-Path -Path $destinationPath))
    {
        throw "Please build the module first."
    }
    Copy-Item -Path $destinationPath -Destination $modulePSSAPath -Recurse -Verbose:$verbosity
}

if ($Test)
{
    Import-Module PSScriptAnalyzer -ErrorAction Stop
    Import-Module -Name Pester -RequiredVersion 3.4.0 -ErrorAction Stop

    
    Function GetTestRunnerScriptContent($testPath)
    {
        $x = @"
        cd $testPath
        Invoke-Pester
"@
        return $x
    }
    
    Function CreateTestRunnerScript($testPath)
    {
        $tmptmpFilePath = [System.IO.Path]::GetTempFileName()
        $tmpFilePath = $tmptmpFilePath + '.ps1'
        Move-Item $tmptmpFilePath $tmpFilePath -Verbose:$verbosity
        $content = GetTestRunnerScriptContent $testPath
        Set-Content -Path $tmpFilePath -Value $content -Verbose:$verbosity
        return $tmpFilePath
    }

    Function GetTestPath($TestType)
    {
        if ($TestType -eq "engine")
        {
            $testPath = Join-Path $projectRoot "Tests/Engine"
        }
        else
        {
            $testPath = Join-Path $projectRoot "Tests/Rules"
        }
        return $testPath
    }
    
    Function RunTest($TestType, $DifferentProcess)
    {
        $testPath = GetTestPath($TestType)        
        if ($DifferentProcess)
        {
            $testScriptFilePath = CreateTestRunnerScript $testPath
            Start-Process powershell -ArgumentList "-NoExit","-File $testScriptFilePath" -Verb runas
        }
        else
        {            
            try
            {
                Push-Location .
                ([scriptblock]::Create((GetTestRunnerScriptContent $testPath))).Invoke()
            }
            finally
            {
                Pop-Location
            }
        }
        # clean up the test file
    }

    if ($Engine)
    {
        RunTest('engine')
    }
    if ($Rules)
    {
        RunTest('rules')
    }
}

if ($Uninstall)
{
    Remove-Item -Path $modulePSSAPath -Force -Verbose:$verbosity -Recurse
}


