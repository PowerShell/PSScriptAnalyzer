[CmdletBinding()]
param(

    [Parameter(ParameterSetName='Build')]
    [ValidateSet('PSV3 Debug','PSV3 Release','Debug','Release')]
    [string] $Configuration = 'Debug',

    [Parameter(ParameterSetName='Build')]
    [switch] $BuildSolution = $false,

    [Parameter(ParameterSetName='Build')]
    [switch] $CleanSolution = $false,

    [Parameter(ParameterSetName='Build')]
    [switch] $BuildDocs = $false,

    [Parameter(ParameterSetName='Build')]
    [switch] $CleanOutput = $false,

    [Parameter(ParameterSetName='Build')]
    [switch] $Install = $false,

    [Parameter(ParameterSetName='Build')]
    [switch] $Uninstall = $false,

    [Parameter(ParameterSetName='Test')]
    [switch] $Test = $false,

    [Parameter(ParameterSetName='Test')]
    [switch] $Engine = $false,

    [Parameter(ParameterSetName='Test')]
    [switch] $Rules = $false,

    [Parameter(ParameterSetName='Test')]
    [switch] $RunInDifferentProcess = $false
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
    if ($null -eq (Get-Module platyPS -ListAvailable -Verbose:$verbosity | Where-Object { $_.Version -ge 0.5 }))
    {
        "Cannot find platyPS. Please install it from https://www.powershellgallery.com/packages/platyPS/ using e.g. the following command: Install-Module platyPS"
    }
    if ((Get-Module platyPS -Verbose:$verbosity) -eq $null)
    {
        Import-Module platyPS -Verbose:$verbosity
    }
    if (-not (Test-Path $markdownDocsPath -Verbose:$verbosity))
    {
        throw "Cannot find markdown documentation folder."
    }
    New-ExternalHelp -Path $markdownDocsPath -OutputPath $outputDocsPath -Force -Verbose:$verbosity
}

# Appveyor errors out due to $profile being null. Hence...
$moduleRootPath = "$HOME/Documents/WindowsPowerShell/Modules"
if ($profile -ne $null)
{
    $moduleRootPath = Join-Path (Split-Path $profile) 'Modules'
}
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
    Import-Module -Name Pester -MinimumVersion 3.4.0 -ErrorAction Stop
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

    Function RunTest($TestType, [Boolean] $DifferentProcess)
    {
        $testPath = GetTestPath($TestType)
        if ($DifferentProcess)
        {
            $testScriptFilePath = CreateTestRunnerScript $testPath
            Start-Process powershell -ArgumentList "-NoExit","-File $testScriptFilePath" -Verb runas
            # clean up the test file
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
    }

    if ($Engine -or (-not ($Engine -or $Rules)))
    {
        RunTest 'engine' $RunInDifferentProcess
    }
    if ($Rules -or (-not ($Engine -or $Rules)))
    {
        RunTest 'rules' $RunInDifferentProcess
    }
}

if ($Uninstall)
{
    Remove-Item -Path $modulePSSAPath -Force -Verbose:$verbosity -Recurse
}