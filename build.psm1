# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Build module for PowerShell ScriptAnalyzer
$projectRoot = $PSScriptRoot
$destinationDir = Join-Path -Path $projectRoot -ChildPath (Join-Path -Path "out" -ChildPath "PSScriptAnalyzer")

function Publish-File
{
    param ([string[]]$itemsToCopy, [string]$destination)
    if (-not (Test-Path $destination))
    {
        $null = New-Item -ItemType Directory $destination -Force
    }
    foreach ($file in $itemsToCopy)
    {
        Copy-Item -Path $file -Destination (Join-Path $destination (Split-Path $file -Leaf)) -Force
    }
}

# attempt to get the users module directory
function Get-UserModulePath
{
    if ( $IsCoreCLR -and ! $IsWindows )
    {
        $platformType = "System.Management.Automation.Platform" -as [Type]
        if ( $platformType ) {
            ${platformType}::SelectProductNameForDirectory("USER_MODULES")
        }
        else {
            throw "Could not determine users module path"
        }
    }
    else {
        "${HOME}/Documents/WindowsPowerShell/Modules"
    }
}


function Uninstall-ScriptAnalyzer
{
    [CmdletBinding(SupportsShouldProcess)]
    param ( $ModulePath = $(Join-Path -Path (Get-UserModulePath) -ChildPath PSScriptAnalyzer) )
    END {
        if ( $PSCmdlet.ShouldProcess("$modulePath") ) {
            Remove-Item -Recurse -Path "$ModulePath" -Force
        }
    }
}

# install script analyzer, by default into the users module path
function Install-ScriptAnalyzer
{
    [CmdletBinding(SupportsShouldProcess)]
    param ( $ModulePath = $(Join-Path -Path (Get-UserModulePath) -ChildPath PSScriptAnalyzer) )
    END {
        if ( $PSCmdlet.ShouldProcess("$modulePath") ) {
            Copy-Item -Recurse -Path "$destinationDir" -Destination "$ModulePath\." -Force
        }
    }
}

# if script analyzer is installed, remove it
function Uninstall-ScriptAnalyzer
{
    [CmdletBinding(SupportsShouldProcess)]
    param ( $ModulePath = $(Join-Path -Path (Get-UserModulePath) -ChildPath PSScriptAnalyzer) )
    END {
        if (Test-Path $ModulePath -and (Get-Item $ModulePath).PSIsContainer )
        {
            Remove-Item -Force -Recurse $ModulePath
        }
    }
}

# Clean up the build location
function Remove-Build
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param ()
    END {
        if ( $PSCmdlet.ShouldProcess("${destinationDir}")) {
            if ( Test-Path ${destinationDir} ) {
                Remove-Item -Force -Recurse ${destinationDir}
            }
        }
    }
}

# Build documentation using platyPS
function Start-DocumentationBuild
{
    $docsPath = Join-Path $projectRoot docs
    $markdownDocsPath = Join-Path $docsPath markdown
    $outputDocsPath = Join-Path $destinationDir en-US
    $requiredVersionOfplatyPS = 0.9
    #$modInfo = new-object Microsoft.PowerShell.Commands.ModuleSpecification -ArgumentList @{ ModuleName = "platyps"; ModuleVersion = $requiredVersionOfplatyPS}
    #if ( $null -eq (Get-Module -ListAvailable -FullyQualifiedName $modInfo))
    if ( $null -eq (Get-Module -ListAvailable platyPS))
    {
        Write-Verbose -verbose "platyPS not found, installing"
        Install-Module -Force -Name platyPS -Scope CurrentUser
        # throw "Cannot find required minimum version $requiredVersionOfplatyPS of platyPS. Install via 'Install-Module platyPS'"
    }
    if (-not (Test-Path $markdownDocsPath))
    {
        throw "Cannot find markdown documentation folder."
    }
    Import-Module platyPS
    if ( ! (Test-Path $outputDocsPath)) {
        $null = New-Item -Type Directory -Path $outputDocsPath -Force
    }
    $null = New-ExternalHelp -Path $markdownDocsPath -OutputPath $outputDocsPath -Force
}

# build script analyzer (and optionally build everything with -All)
function Start-ScriptAnalyzerBuild
{
    [CmdletBinding(DefaultParameterSetName="BuildOne")]
    param (
        [Parameter(ParameterSetName="BuildAll")]
        [switch]$All,

        [Parameter(ParameterSetName="BuildOne")]
        [ValidateSet("full", "core")]
        [string]$Framework = "core",

        [Parameter(ParameterSetName="BuildOne")]
        [ValidateSet("3","4","5")]
        [string]$PSVersion = "5",

        [Parameter(ParameterSetName="BuildOne")]
        [Parameter(ParameterSetName="BuildAll")]
        [ValidateSet("Debug", "Release")]
        [string]$Configuration = "Debug",

        [Parameter(ParameterSetName="BuildDoc")]
        [switch]$Documentation
        )

    BEGIN {
        if ( $PSVersion -match "[34]" -and $Framework -eq "core" ) {
            throw "Script Analyzer for PowerShell 3/4 cannot be built for framework 'core'"
        }
    }
    END {
        if ( $All )
        {
            # Build all the versions of the analyzer
            Start-ScriptAnalyzerBuild -Framework full -Configuration $Configuration -PSVersion "3"
            Start-ScriptAnalyzerBuild -Framework full -Configuration $Configuration -PSVersion "4"
            Start-ScriptAnalyzerBuild -Framework full -Configuration $Configuration -PSVersion "5"
            Start-ScriptAnalyzerBuild -Framework core -Configuration $Configuration -PSVersion "5"
            Start-ScriptAnalyzerBuild -Documentation
            return
        }

        if ( $Documentation )
        {
            Start-DocumentationBuild
            return
        }

        Push-Location -Path $projectRoot

        if ( $framework -eq "core" ) {
            $frameworkName = "netstandard2.0"
        }
        else {
            $frameworkName = "net451"
        }

        # build the appropriate assembly
        if ($PSVersion -match "[34]" -and $Framework -eq "core")
        {
            throw ("ScriptAnalyzer for PS version '{0}' is not applicable to {1} framework" -f $PSVersion,$Framework)
        }

        #Write-Progress "Building ScriptAnalyzer"
        if (-not (Test-Path "$projectRoot/global.json"))
        {
            throw "Not in solution root"
        }

        $itemsToCopyCommon = @(
            "$projectRoot\Engine\PSScriptAnalyzer.psd1", "$projectRoot\Engine\PSScriptAnalyzer.psm1",
            "$projectRoot\Engine\ScriptAnalyzer.format.ps1xml", "$projectRoot\Engine\ScriptAnalyzer.types.ps1xml"
            )

        $destinationDir = "$projectRoot\out\PSScriptAnalyzer"
        # this is normalizing case as well as selecting the proper location
        if ( $Framework -eq "core" ) {
            $destinationDirBinaries = "$destinationDir\coreclr"
        }
        elseif ($PSVersion -eq '3') {
            $destinationDirBinaries = "$destinationDir\PSv3"
        }
        elseif ($PSVersion -eq '4') {
            $destinationDirBinaries = "$destinationDir\PSv4"
        }
        else {
            $destinationDirBinaries = $destinationDir
        }

        # build the analyzer
        #Write-Progress "Building for framework $Framework, configuration $Configuration"
        # The Rules project has a dependency on the Engine therefore just building the Rules project is enough
        $config = "PSV${PSVersion}${Configuration}"
        try {
            Push-Location $projectRoot/Rules
            Write-Progress "Building ScriptAnalyzer '$framework' version '${PSVersion}' configuration '${Configuration}'"
            $buildOutput = dotnet build Rules.csproj --framework $frameworkName --configuration "${config}"
            if ( $LASTEXITCODE -ne 0 ) { throw "$buildOutput" }
        }
        catch {
            Write-Error "Failure to build $framework ${config}"
            return
        }
        finally {
            Pop-Location
        }

        #Write-Progress "Copying files to $destinationDir"
        Publish-File $itemsToCopyCommon $destinationDir

        $itemsToCopyBinaries = @(
            "$projectRoot\Engine\bin\${config}\${frameworkName}\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
            "$projectRoot\Rules\bin\${config}\${frameworkName}\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll"
            )
        Publish-File $itemsToCopyBinaries $destinationDirBinaries

        $settingsFiles = Get-Childitem "$projectRoot\Engine\Settings" | ForEach-Object -MemberName FullName
        Publish-File $settingsFiles (Join-Path -Path $destinationDir -ChildPath Settings)

        # copy newtonsoft dll if net451 framework
        if ($Framework -eq "full") {
            Copy-Item -path "$projectRoot\Rules\bin\${config}\${frameworkName}\Newtonsoft.Json.dll" -Destination $destinationDirBinaries
        }

        Pop-Location
    }
}

# TEST HELPERS
# Run our tests
function Test-ScriptAnalyzer
{
    [CmdletBinding()]
    param ( [Parameter()][switch]$InProcess )

    END {
        $testModulePath = Join-Path "${projectRoot}" -ChildPath out
        $testResultsFile = Join-Path ${projectRoot} -childPath TestResults.xml
        $testScripts = "${projectRoot}\Tests\Engine,${projectRoot}\Tests\Rules,${projectRoot}\Tests\Documentation"
        try {
            $savedModulePath = $env:PSModulePath
            $env:PSModulePath = "${testModulePath}{0}${env:PSModulePath}" -f [System.IO.Path]::PathSeparator
            $scriptBlock = [scriptblock]::Create("Invoke-Pester -Path $testScripts -OutputFormat NUnitXml -OutputFile $testResultsFile -Show Describe")
            if ( $InProcess ) {
                & $scriptBlock
            }
            else {
                $powershell = (Get-Process -id $PID).MainModule.FileName
                & ${powershell} -Command $scriptBlock
            }
        }
        finally {
            $env:PSModulePath = $savedModulePath
        }
    }
}

# a simple function to make it easier to retrieve the test results
function Get-TestResults
{
    param ( $logfile = (Join-Path -Path ${projectRoot} -ChildPath TestResults.xml) )
    $logPath = (Resolve-Path $logfile).Path
    $results = [xml](Get-Content $logPath)
    $results.SelectNodes(".//test-case")
}

# a simple function to make it easier to retrieve the failures
# it's not a filter of the results of Get-TestResults because this is faster
function Get-TestFailures
{
    param ( $logfile = (Join-Path -Path ${projectRoot} -ChildPath TestResults.xml) )
    $logPath = (Resolve-Path $logfile).Path
    $results = [xml](Get-Content $logPath)
    $results.SelectNodes(".//test-case[@result='Failure']")
}
