# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Copy of https://github.com/TravisEz13/PSPackageProject/blob/master/src/PSPackageProject.psm1
# References to PackageManagement cmdlets in this file have reproduced deadlocks in PSSA most consistently
# See https://github.com/PowerShell/PSScriptAnalyzer/issues/1297
# For the specific lines in this file that can cause deadlocks, search NOTE in this file

#region Private implementation functions

function GetPowerShellName {
    switch ($PSVersionTable.PSEdition) {
        'Core' {
            return 'PowerShell Core'
        }

        default {
            return 'Windows PowerShell'
        }
    }
}

function Test-ConfigFile {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if ($Path.EndsWith('pspackageproject.json') -and (Test-Path $Path -PathType Leaf)) {
        return $true
    }
}

function SearchConfigFile {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $startPath = $Path

    do {
        $configPath = Join-Path $startPath 'pspackageproject.json'

        if (-not (Test-Path $configPath)) {
            $startPath = Split-Path $startPath
        }
        else {
            return $configPath
        }
    } while ($newPath -ne '')
}

function Join-Path2 {
    param(
        [Parameter(Mandatory)]
        [string[]] $Path,

        [Parameter(Mandatory)]
        [string] $ChildPath,

        [string[]] $AdditionalChildPath
    )

    $paths = [System.Collections.ArrayList]::new()

    $Path | ForEach-Object { $null = $paths.Add($_) }
    $null = $paths.Add($ChildPath)
    $AdditionalChildPath | ForEach-Object { $null = $paths.Add($_) }

    [System.IO.Path]::Combine($paths)
}

function RunPwshCommandInSubprocess {
    param(
        [string]
        $Command
    )

    if (-not $script:pwshPath) {
        $script:pwshPath = (Get-Process -Id $PID).Path
    }

    & $script:pwshPath -NoProfile -NoLogo -Command $Command
}

function RunProjectBuild {
    param(
        [string]
        $ProjectRoot
    )

    & "$ProjectRoot/build.ps1" -Clean -Build
}

function GetHelpPath {
    param(
        [cultureinfo]
        $Culture,

        [string]
        $ProjectRoot
    )

    $ProjectRoot = Resolve-Path -Path $ProjectRoot

    $config = Get-PSPackageProjectConfiguration

    $cultureName = $Culture.Name

    return (Join-Path2 $ProjectRoot $config.HelpPath $cultureName)
}

function GetOutputModulePath {
    $config = Get-PSPackageProjectConfiguration
    return (Join-Path $config.BuildOutputPath $config.ModuleName)
}

function HasCmdletHelp {
    param(
        [string]
        $HelpResourcePath
    )

    if (-not (Test-Path -Path $HelpResourcePath)) {
        return $false
    }

    $files = Get-ChildItem -Path $HelpResourcePath -ErrorAction Ignore |
    Where-Object Name -Like '*.md' |
    Where-Object Name -NotLike 'about_*'

    return $files.Count -gt 0
}

function Initialize-CIYml {
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    $boilerplateCIYml = Join-Path2 -Path $PSScriptRoot -ChildPath 'yml' -AdditionalChildPath 'ci_for_init.yml'
    $destYmlPath = New-Item (Join-Path -Path $Path -ChildPath '.ci') -ItemType Directory
    Copy-Item $boilerplateCIYml -Destination (Join-Path $destYmlPath -ChildPath 'ci.yml') -Force

    $boilerplateTestYml = Join-Path2 -Path $PSScriptRoot -ChildPath 'yml' -AdditionalChildPath 'test_for_init.yml'
    Copy-Item $boilerplateTestYml -Destination (Join-Path $destYmlPath -ChildPath 'test.yml') -Force

    $boilerplateReleaseYml = Join-Path2 -Path $PSScriptRoot -ChildPath 'yml' -AdditionalChildPath 'release_for_init.yml'
    Copy-Item $boilerplateTestYml -Destination (Join-Path $destYmlPath -ChildPath 'release.yml') -Force
}

function Test-PSPesterResults
{
    [CmdletBinding()]
    param(
        [Parameter()]
        [string] $TestResultsFile = "pester-tests.xml"
    )

    if (!(Test-Path $TestResultsFile)) {
        throw "Test result file '$testResultsFile' not found for $TestArea."
    }

    $x = [xml](Get-Content -raw $testResultsFile)
    if ([int]$x.'test-results'.failures -gt 0) {
        Write-Error "TEST FAILURES"
        # switch between methods, SelectNode is not available on dotnet core
        if ( "System.Xml.XmlDocumentXPathExtensions" -as [Type] ) {
            $failures = [System.Xml.XmlDocumentXPathExtensions]::SelectNodes($x."test-results", './/test-case[@result = "Failure"]')
        }
        else {
            $failures = $x.SelectNodes('.//test-case[@result = "Failure"]')
        }
        foreach ( $testfail in $failures ) {
            Show-PSPesterError -testFailure $testfail
        }
    }
}

function Show-PSPesterError {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)]
        [Xml.XmlElement]$testFailure
    )


    $description = $testFailure.description
    $name = $testFailure.name
    $message = $testFailure.failure.message
    $stackTrace = $testFailure.failure."stack-trace"

    $fullMsg = "`n{0}`n{1}`n{2}`n{3}`{4}" -f ("Description: " + $description), ("Name:        " + $name), "message:", $message, "stack-trace:", $stackTrace

    Write-Error $fullMsg
}

function Invoke-FunctionalValidation {
    param ( $tags = "CI" )
    $config = Get-PSPackageProjectConfiguration
    try {
        $testResultFile = "result.pester.xml"
        $testPath = $config.TestPath
        $modStage = "{0}/{1}" -f $config.BuildOutputPath,$config.ModuleName
        $command = "import-module ${modStage} -Force -Verbose; Set-Location $testPath; Invoke-Pester -Path . -OutputFile ${testResultFile} -tags '$tags'"
        $output = RunPwshCommandInSubprocess -command $command | Foreach-Object { Write-Verbose -Verbose $_ }
        return (Join-Path ${testPath} "$testResultFile")
    }
    catch {
        $output | Foreach-Object { Write-Warning "$_" }
        Write-Error "Error invoking tests"
    }
}

function Invoke-StaticValidation {
    $fault = $false

    $config = Get-PSPackageProjectConfiguration

    Write-Verbose "Running ScriptAnalyzer" -Verbose
    $resultPSSA = RunScriptAnalysis -Location $config.BuildOutputPath

    Write-Verbose -Verbose "PSSA result file: $resultPSSA"

    Test-PSPesterResults -TestResultsFile $resultPSSA

    Write-Verbose "Running BinSkim" -Verbose
    $resultBinSkim = Invoke-BinSkim -Location (Join-Path2 -Path $config.BuildOutputPath -ChildPath $config.ModuleName)

    Test-PSPesterResults -TestResultsFile $resultBinSkim
}

function RunScriptAnalysis {
    try {
        Push-Location

        $pssaParams = @{
            Severity = 'Warning', 'ParseError'
            Path     = GetOutputModulePath
            Recurse  = $true
        }

        $results = Invoke-ScriptAnalyzer @pssaParams
        if ( $results ) {
            $xmlPath = ConvertPssaDiagnosticsToNUnit -Diagnostic $results
            # send back the xml file path.
            $xmlPath
            if ($env:TF_BUILD) {
                $powershellName = GetPowerShellName
                Publish-AzDevOpsTestResult -Path $xmlPath -Title "PSScriptAnalyzer $env:AGENT_OS - $powershellName Results" -Type NUnit
            }
        }
    }
    finally {
        Pop-Location
    }
}

function ConvertPssaDiagnosticsToNUnit {
    param(
        [Parameter(ValueFromPipeline)]
        [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
        $Diagnostic
    )

    $sb = [System.Text.StringBuilder]::new()
    $null = $sb.Append("Describe 'PSScriptAnalyzer Diagnostics' { `n")
    foreach ($d in $Diagnostic) {
        $severity = $d.Severity
        $ruleName = $d.RuleName
        $message = $d.Message -replace "'", "``"
        $description = "[$severity] ${ruleName}: $message"
        $null = $sb.Append("It '$ruleName' { `nthrow '$message' }`n")
    }
    $null = $sb.Append('}')

    $testPath = Join-Path ([System.IO.Path]::GetTempPath()) "pssa.tests.ps1"
    $xmlPath = Join-Path ([System.IO.Path]::GetTempPath()) "pssa.xml"

    try {
        Set-Content -Path $testPath -Value $sb.ToString()
        Invoke-Pester -Script $testPath -OutputFormat NUnitXml -OutputFile $xmlPath
    }
    finally {
        Remove-Item -Path $testPath -Force
    }

    return $xmlPath
}

<#
.SYNOPSIS
Generates help file stubs.

.DESCRIPTION
Generates stubs for about_*.md help documentation for a given module.

.PARAMETER ProjectRoot
The repository root directory path.

.PARAMETER ModuleName
The name of the module to generate help for.

.PARAMETER Culture
The culture or locale the help is to be generated in/for.
#>
function Initialize-PSPackageProjectHelp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $ProjectRoot,

        [Parameter(Mandatory)]
        [string]
        $ModuleName,

        [Parameter()]
        [cultureinfo]
        $Culture = [cultureinfo]::CurrentCulture
    )

    $ProjectRoot = Resolve-Path -Path $ProjectRoot

    $helpResourcePath = GetHelpPath -ProjectRoot $ProjectRoot -Culture $Culture

    $null = New-Item -Path $helpResourcePath -ItemType Directory -ErrorAction Stop

    New-MarkdownAboutHelp -OutputFolder $helpResourcePath -AboutName $ModuleName
}

#endregion Private implementation functions

#region Public commands

function Invoke-PSPackageProjectTest {
    param(
        [Parameter()]
        [ValidateSet("Functional", "StaticAnalysis")]
        [string[]]
        $Type
    )

    END {
        $config = Get-PSPackageProjectConfiguration
        if ($Type -contains "Functional" ) {
            # this will return a path to the results
            $resultFile = Invoke-FunctionalValidation
            Test-PSPesterResults -TestResultsFile $resultFile
            $powershellName = GetPowerShellName
            Publish-AzDevOpsTestResult -Path $resultFile -Title "Functional Tests -  $env:AGENT_OS - $powershellName Results" -Type NUnit
        }

        if ($Type -contains "StaticAnalysis" ) {
            Invoke-StaticValidation
        }
    }
}

function Invoke-BinSkim {
    [CmdletBinding(DefaultParameterSetName = 'byPath')]
    param(
        [Parameter(ParameterSetName = 'byPath', Mandatory)]
        [string]
        $Location,
        [Parameter(ParameterSetName = 'byPath')]
        [string]
        $Filter = '*'
    )

    $testscript = @'
Describe "BinSkim" {
    BeforeAll{
        $outputPath =  Join-Path -Path ([System.io.path]::GetTempPath()) -ChildPath 'pspackageproject-results.json'
        if(Test-Path $outputPath)
        {
            $results = Get-Content $outputPath | ConvertFrom-Json
        }
    }

    foreach($file in $results.runs.files.PsObject.Properties.Name)
    {
        foreach($rule in $results.runs.rules.psobject.properties.name)
        {
            $fileResults = @($results.runs.results |
                Where-Object {
                    Write-Verbose "$($_.ruleId) -eq $rule"
                    $_.locations.analysisTarget.uri -eq $File -and $_.ruleId -eq $rule})

            $message = $null
            if($fileResults.Count -ne 0) {
                $fileResult = $fileResults[0]
                $message = $results.runs.rules.$rule.messageFormats.($fileResult.Level) -f ($fileResult.formattedRuleMessage.arguments)
            }

            if($message){
                it "$file should not have errors for " {
                    throw $message
                }
            }
        }
    }
}
'@
    $eligbleFiles = @(Get-ChildItem -Path $Location -Filter $Filter -Recurse -File | Where-Object { $_.Extension -in '.exe','.dll','','.so','.dylib'})
    if($eligbleFiles.Count -ne 0)
    {
        $sourceName = 'Nuget'
        Register-PackageSource -ProviderName NuGet -Name $sourceName -Location https://api.nuget.org/v3/index.json -erroraction ignore
        $packageName = 'microsoft.codeanalysis.binskim'
        $packageLocation = Join-Path2 -Path ([System.io.path]::GetTempPath()) -ChildPath 'pspackageproject-packages'
        if ($IsLinux) {
            $binaryName = 'BinSkim'
            $rid = 'linux-x64'
        }
        elseif ($IsWindows -ne $false) {
            $binaryName = 'BinSkim.exe'
            if ([Environment]::Is64BitOperatingSystem) {
                $rid = 'win-x64'
            }
            else {
                $rid = 'win-x86'
            }
        }
        else {
            Write-Warning "unsupported platform"
            return
        }

        Write-Verbose "Finding binskim..." -Verbose
        # NOTE: Deadlock occurs here on Find-Package
        $packageInfo = Find-Package -Name $packageName -Source $sourceName
        $dirName = $packageInfo.Name + '.' + $packageInfo.Version
        $toolLocation = Join-Path2 -Path $packageLocation -ChildPath $dirName -AdditionalChildPath 'tools', 'netcoreapp2.0', $rid, $binaryName
        if (!(test-path -path $toolLocation)) {
            Write-Verbose "Installing binskim..." -Verbose
            # NOTE: Deadlock occurs here on Install-Package
            $packageInfo | Install-Package -Destination $packageLocation -Force
        }

        if ($IsLinux) {
            chmod a+x $toolLocation
        }

        $resolvedPath = (Resolve-Path -Path $Location).ProviderPath
        $toAnalyze = Join-Path2 -Path $resolvedPath -ChildPath $Filter

        $outputPath = Join-Path2 -Path ([System.io.path]::GetTempPath()) -ChildPath 'pspackageproject-results.json'
        Write-Verbose "Running binskim..." -Verbose
        & $toolLocation analyze $toAnalyze --output $outputPath --pretty-print --recurse  > binskim.log 2>&1
        Write-Verbose "binskim exitcode: $LASTEXITCODE" -Verbose
        $PowerShellName = GetPowerShellName
        Publish-Artifact -Path ./binskim.log -Name "binskim-log-${env:AGENT_OS}-${PowerShellName}"

        $testsPath = Join-Path2 -Path ([System.io.path]::GetTempPath()) -ChildPath 'pspackageproject' -AdditionalChildPath 'BinSkim', 'binskim.tests.ps1'

        $null = New-Item -ItemType Directory -Path (Split-Path $testsPath)

        $testscript | Out-File $testsPath -Force

        Write-Verbose "Generating test results..." -Verbose

        Invoke-Pester -Script $testsPath -OutputFile ./binskim-results.xml -OutputFormat NUnitXml

        Publish-AzDevOpsTestResult -Path ./binskim-results.xml -Title "BinSkim $env:AGENT_OS - $PowerShellName Results" -Type NUnit
        return ./binskim-results.xml
    }
}

function Publish-AzDevOpsTestResult {
    param(
        [parameter(Mandatory)]
        [string]
        $Path,
        [parameter(Mandatory)]
        [string]
        $Title,
        [string]
        $Type = 'NUnit'
    )

    $artifactPath = (Resolve-Path $Path).ProviderPath

    Write-Verbose -Verbose "Uploading $artifactPath"

    # Just do nothing if we are not in AzDevOps
    if ($env:TF_BUILD) {
        Write-Host "##vso[results.publish type=$Type;mergeResults=true;runTitle=$Title;publishRunAttachments=true;resultFiles=$artifactPath;failTaskOnFailedTests=true;]"
    }
}

<#
.SYNOPSIS
Create or update cmdlet help stubs markdown files.

.DESCRIPTION
Creates or updates the cmdlet help resource files
for the given module.
The generated help will be stubs, requiring regions with {{ }}
to be filled in.

.PARAMETER ProjectRoot
The path to the repository root of the module.

.PARAMETER ModuleName
The name of the module to generate cmdlet help for.

.PARAMETER Culture
The culture in which cmdlet help should be created.
#>
function Add-PSPackageProjectCmdletHelp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $ProjectRoot,

        [Parameter(Mandatory)]
        [string]
        $ModuleName,

        [Parameter()]
        [cultureinfo]
        $Culture
    )

    $ProjectRoot = Resolve-Path -Path $ProjectRoot

    $helpResourcePath = GetHelpPath -ProjectRoot $ProjectRoot -Culture $Culture

    RunProjectBuild -ProjectRoot $ProjectRoot

    $outModulePath = GetOutputModulePath -ProjectRoot $ProjectRoot -ModuleName $ModuleName

    if (-not (HasCmdletHelp -HelpResourcePath $helpResourcePath)) {
        New-Item -Path $helpResourcePath -ItemType Directory -ErrorAction Ignore
        RunPwshCommandInSubprocess -Command "Import-Module '$outModulePath'; New-MarkdownHelp -Module $ModuleName -OutputFolder '$helpResourcePath'"
        return
    }

    RunPwshCommandInSubprocess -Command "Import-Module '$outModulePath'; Update-MarkdownHelp -Path '$helpResourcePath'"
}

<#
.SYNOPSIS
Assembles help files into staging output.

.DESCRIPTION
Compiles markdown help resources into
PowerShell external help files and places
them into the staging location.

.PARAMETER ProjectRoot
The path to the project repository root.

.PARAMETER ModuleName
The name of the module to publish help for.

.PARAMETER Culture
The locale or culture the help is written for.
#>
function Export-PSPackageProjectHelp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $ProjectRoot,

        [Parameter(Mandatory)]
        [string]
        $ModuleName,

        [Parameter()]
        [cultureinfo]
        $Culture = [cultureinfo]::CurrentCulture
    )

    $cultureName = $Culture.Name

    $helpResourcePath = GetHelpPath -ProjectRoot $ProjectRoot -Culture $Culture

    $outModulePath = GetOutputModulePath -ProjectRoot $ProjectRoot -ModuleName $ModuleName

    New-ExternalHelp -Path $helpResourcePath -OutputPath "$outModulePath/$cultureName" -Force
}

function Invoke-PSPackageProjectBuild {
    [CmdletBinding()]
    param(
        [Parameter()]
        [ScriptBlock]
        $BuildScript
    )

    Write-Verbose -Verbose "Invoking build script"

    $BuildScript.Invoke()

    New-PSPackageProjectPackage

    Write-Verbose -Verbose "Finished invoking build script"
}

function New-PSPackageProjectPackage
{
    Write-Verbose -Message "Starting New-PSPackageProjectPackage" -Verbose
    $ErrorActionPreference = 'Stop'
    $config = Get-PSPackageProjectConfiguration
    $modulePath = Join-Path2 -Path $config.BuildOutputPath -ChildPath $config.ModuleName
    $sourceName = 'pspackageproject-local-repo'
    $packageLocation = Join-Path2 -Path ([System.io.path]::GetTempPath()) -ChildPath $sourceName
    $modulesLocation = Join-Path2 -Path $packageLocation -ChildPath 'modules'

    if (Test-Path $modulesLocation) {
        Remove-Item $modulesLocation -Recurse -Force -ErrorAction Ignore
    }

    $null = New-Item -Path $modulesLocation -Force -ItemType Directory
    $scriptsLocation = $modulesLocation

    Write-Verbose -Message "Starting dependency download" -Verbose

    # TODO : dynamically detect module dependecies and save them
    Save-Package2 -Name PlatyPs, Pester -Location $modulesLocation
    Save-Package2 -Name PSScriptAnalyzer -RequiredVersion '1.18.0' -Location $modulesLocation

    Write-Verbose -Message "Dependency download complete" -Verbose
    if (!(Get-PSRepository -Name $sourceName -ErrorAction Ignore)) {
        Register-PSRepository -Name $sourceName -SourceLocation $modulesLocation -PublishLocation $modulesLocation
    }

    Write-Verbose -Verbose "Starting to publish module: $modulePath"
    Publish-Module -Path $modulePath -Repository $sourceName -NuGetApiKey 'fake' -Force

    Write-Verbose -Message "Local package published" -Verbose

    $nupkgPath = (Get-ChildItem -Path $modulesLocation -Filter "$($config.ModuleName)*.nupkg").FullName
    Publish-Artifact -Path $nupkgPath -Name nupkg

    Write-Verbose -Message "Starting New-PSPackageProjectPackage" -Verbose
}

# Wrapper to push artifact
function Publish-Artifact
{
    param(
        [Parameter(Mandatory)]
        [ValidateScript({Test-Path -Path $_})]
        $Path,
        [string]
        $Name
    )

    $resolvedPath = (Resolve-Path -Path $Path).ProviderPath

    if(!$Name)
    {
        $artifactName = [system.io.path]::GetFileName($Path)
    }
    else
    {
        $artifactName = $Name
    }

    if ($env:TF_BUILD) {
        # In Azure DevOps
        Write-Host "##vso[artifact.upload containerfolder=$artifactName;artifactname=$artifactName;]$resolvedPath"
    }
}

function Save-Package2
{
    param(
        [string[]]
        $Name,
        [String]
        $Location,
        [string]
        $RequiredVersion
    )

    if($RequiredVersion) {
        # NOTE: Deadlock occurs here on Save-Package
        Save-Package -Name $Name -Source 'https://www.powershellgallery.com/api/v2' -Path $Location -ProviderName NuGet -RequiredVersion $RequiredVersion
    } else {
        # NOTE: Deadlock occurs here on Save-Package
        Save-Package -Name $Name -Source 'https://www.powershellgallery.com/api/v2' -Path $Location -ProviderName NuGet
    }

}

function Initialize-PSPackageProject {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$ModuleName,
        [string]$ModuleBase = ".",
        [switch]$Force
    )

    if ( [System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($ModuleBase) ) {
        throw "Modulebase '${ModuleBase}' contains wildcards"
    }

    $ModuleRoot = (Resolve-Path $ModuleBase -ea SilentlyContinue ).Path
    if ( $ModuleRoot -and ! $force ) {
        throw "'${ModuleRoot}' already exists, use -Force to overwrite"
    }

    if ( ! $ModuleRoot ) {
        $ModuleRoot = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($ModuleBase)
    }
    $null = New-Item -ItemType Directory -Path $ModuleRoot
    $ModuleInfo = @{
        ModuleName = $ModuleName
        ModuleRoot = $ModuleRoot
    }

    # Create the help directory
    # and populate a couple of files
    Initialize-PSPackageProjectHelp -ProjectRoot $ModuleRoot -ModuleName $ModuleName

    # Create the scaffold for .psd1 and .psm1
    $moduleSourceBase = Join-Path $ModuleRoot "src"
    $null = New-Item -ItemType Directory -Path $moduleSourceBase
    $moduleFileWithoutExtension = Join-Path $moduleSourceBase ${ModuleName}
    New-ModuleManifest -Path "${moduleFileWithoutExtension}.psd1" -CmdletsToExport "verb-noun" -RootModule "./${ModuleName}.dll"
    $null = New-Item -Type File "${moduleFileWithoutExtension}.psm1"

    # Create a directory for cs sources and create a classlib csproj file with
    # System.Management.Automation as a package reference
    $moduleCodeBase = Join-Path $moduleSourceBase "code"
    $null = New-Item -ItemType Directory -Path $moduleCodeBase
    try {
        Push-Location $moduleCodeBase
        $output = dotnet new classlib -f netstandard2.0 --no-restore --force
        $output += dotnet add package PowerShellStandard.Library --no-restore
        Move-Item code.csproj "${ModuleName}.csproj"
        @"
using System;
using System.Management.Automation;

namespace ${ModuleName}
{
    [Cmdlet("verb","noun")]
    public class Cmdlet1 : PSCmdlet
    {
        [Parameter(Mandatory=true,Position=0)]
        public string Name {get;set;}

        protected override void ProcessRecord()
        {
            WriteObject(Name);
        }
    }
}
"@ > Class1.cs
    }
    finally {
        Pop-Location
    }

    # make test folder and create a test template
    $testDir = Join-Path $moduleRoot 'test'
    $testTemplate = Join-Path $testDir "${moduleName}.Tests.ps1"
    $null = New-Item -ItemType Directory -Path "${testDir}"
    @"
Describe "Test ${moduleName}" -tags CI {
    BeforeAll {
    }
    BeforeEach {
    }
    AfterEach {
    }
    AfterAll {
    }
    It "This is the first test for ${moduleName}" {
        `$name = "Hello World"
        verb-noun -name `$name | Should -BeExactly `$name
    }
}
"@ | Out-File "${testTemplate}"

    # make CI ymls
    Initialize-CIYml -Path ${moduleRoot}

    # make build.ps1
    $boilerplateBuildScript = Join-Path -Path $PSScriptRoot -ChildPath 'build_for_init.ps1'
    Copy-Item $boilerplateBuildScript -Destination (Join-Path $ModuleRoot -ChildPath 'build.ps1') -Force

    # make pspackageproject.json
    $jsonPrj =
    @{
        SourcePath = "src"
        ModuleName = "${ModuleName}"
        TestPath = 'test'
        HelpPath = 'help'
        BuildOutputPath = 'out'
        Culture = [CultureInfo]::CurrentCulture.Name # This needs to be settable
    } | ConvertTo-Json

    if ($(${PSVersionTable}.PSEdition) -eq 'Desktop') {
        Write-Warning -Message "UTF-8 characters for module name are not supported in Windows PowerShell."
        $jsonPrj | Out-File (Join-Path ${moduleRoot} "pspackageproject.json") -Encoding ascii
    }
    else {
        $jsonPrj | Out-File (Join-Path ${moduleRoot} "pspackageproject.json") -Encoding utf8NoBOM
    }
}

function Get-PSPackageProjectConfiguration {
    param(
        [Parameter()]
        [string] $ConfigPath = "."
    )

    $resolvedPath = Resolve-Path $ConfigPath

    $foundConfigFilePath = if (Test-Path $resolvedPath -PathType Container) {
        SearchConfigFile -Path $resolvedPath
    }
    else {
        if (Test-ConfigFile -Path $resolvedPath) {
            $resolvedPath
        }
    }

    if (Test-Path $foundConfigFilePath) {
        $configObj = Get-Content -Path $foundConfigFilePath | ConvertFrom-Json

        # Populate with full paths

        $projectRoot = Split-Path $foundConfigFilePath

        $configObj.SourcePath = Join-Path $projectRoot -ChildPath $configObj.SourcePath
        $configObj.TestPath = Join-Path $projectRoot -ChildPath $configObj.TestPath
        $configObj.HelpPath = Join-Path $projectRoot -ChildPath $configObj.HelpPath
        $configObj.BuildOutputPath = Join-Path $projectRoot -ChildPath $configObj.BuildOutputPath

        $configObj
    }
    else {
        throw "'pspackageproject.json' not found at: $resolvePath or any or its parent"
    }
}

#endregion Public commands
