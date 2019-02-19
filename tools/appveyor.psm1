# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

$ErrorActionPreference = 'Stop'

# Implements the AppVeyor 'install' step and installs the required versions of Pester, platyPS and the .Net Core SDK if needed.
function Invoke-AppVeyorInstall {
    $requiredPesterVersion = '4.4.4'
    $pester = Get-Module Pester -ListAvailable | Where-Object { $_.Version -eq $requiredPesterVersion }
    if ($null -eq $pester) {
        if ($null -eq (Get-Module -ListAvailable PowershellGet)) {
            # WMF 4 image build
            Write-Verbose -Verbose "Installing Pester via nuget"
            nuget install Pester -Version $requiredPesterVersion -source https://www.powershellgallery.com/api/v2 -outputDirectory "$env:ProgramFiles\WindowsPowerShell\Modules\." -ExcludeVersion
        }
        else {
            # Visual Studio 2017 build (has already Pester v3, therefore a different installation mechanism is needed to make it also use the new version 4)
            Write-Verbose -Verbose "Installing Pester via Install-Module"
            Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
        }
    }

    $platyPSVersion = '0.12.0'
    if ($null -eq (Get-Module -ListAvailable PowershellGet)) {
        # WMF 4 image build
        Write-Verbose -Verbose "Installing platyPS via nuget"
        nuget install platyPS -Version $platyPSVersion -source https://www.powershellgallery.com/api/v2 -outputDirectory "$Env:ProgramFiles\WindowsPowerShell\Modules\." -ExcludeVersion
    }
    else {
        Write-Verbose -Verbose "Installing platyPS via Install-Module"
        Install-Module -Name platyPS -Force -Scope CurrentUser -RequiredVersion $platyPSVersion
    }

    # the build script sorts out the problems of WMF4 and earlier versions of dotnet CLI
    Write-Verbose -Verbose "Installing required .Net CORE SDK"
    Write-Verbose "& $buildScriptDir/build.ps1 -bootstrap"
    $buildScriptDir = (Resolve-Path "$PSScriptRoot/..").Path
    & "$buildScriptDir/build.ps1" -bootstrap
}

# Implements AppVeyor 'test_script' step
function Invoke-AppveyorTest {
    Param(
        [Parameter(Mandatory)]
        [ValidateScript( {Test-Path $_})]
        $CheckoutPath
    )

    # enforce the language to utf-8 to avoid issues
    $env:LANG = "en_US.UTF-8"
    Write-Verbose -Verbose ("Running tests on PowerShell version " + $PSVersionTable.PSVersion)
    Write-Verbose -Verbose "Language set to '${env:LANG}'"

    $modulePath = $env:PSModulePath.Split([System.IO.Path]::PathSeparator) | Where-Object { Test-Path $_} | Select-Object -First 1
    Copy-Item "${CheckoutPath}\out\PSScriptAnalyzer" "$modulePath\" -Recurse -Force
    $testResultsPath = Join-Path ${CheckoutPath} TestResults.xml
    $testScripts = "${CheckoutPath}\Tests\Engine","${CheckoutPath}\Tests\Rules","${CheckoutPath}\Tests\Documentation"
    $uploadUrl = "https://ci.appveyor.com/api/testresults/nunit/${env:APPVEYOR_JOB_ID}"
    $testResults = Invoke-Pester -Script $testScripts -OutputFormat NUnitXml -OutputFile $testResultsPath -PassThru
    Write-Verbose -Verbose "Uploading test results '$testResultsPath' to '${uploadUrl}'"
    [byte[]]$response = (New-Object 'System.Net.WebClient').UploadFile("$uploadUrl" , $testResultsPath)
    if ($testResults.FailedCount -gt 0) {
        throw "$($testResults.FailedCount) tests failed."
    }
}

# Implements AppVeyor 'on_finish' step
function Invoke-AppveyorFinish {
    $stagingDirectory = (Resolve-Path ..).Path
    $zipFile = Join-Path $stagingDirectory "$(Split-Path $pwd -Leaf).zip"
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    [System.IO.Compression.ZipFile]::CreateFromDirectory((Join-Path $pwd 'out'), $zipFile)
    @(
        # add test results as an artifact
        (Get-ChildItem TestResults.xml)
        # You can add other artifacts here
        (Get-ChildItem $zipFile)
    ) | ForEach-Object { Push-AppveyorArtifact $_.FullName }
}
