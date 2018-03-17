# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Implements the AppVeyor 'install' step and installs the required versions of Pester, platyPS and the .Net Core SDK if needed.
function Invoke-AppVeyorInstall {
    $requiredPesterVersion = '4.3.1'
    $pester = Get-Module Pester -ListAvailable | Where-Object { $_.Version -eq $requiredPesterVersion }
    if ($null -eq $pester) {
        if ($null -eq (Get-Module -ListAvailable PowershellGet)) {
            # WMF 4 image build
            nuget install Pester -Version $requiredPesterVersion -source https://www.powershellgallery.com/api/v2 -outputDirectory "$Env:ProgramFiles\WindowsPowerShell\Modules\." -ExcludeVersion
        }
        else {
            # Visual Studio 2017 build (has already Pester v3, therefore a different installation mechanism is needed to make it also use the new version 4)
            Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
        }
    }

    Install-Module -Name platyPS -Force -Scope CurrentUser -RequiredVersion '0.9.0'

    # the legacy WMF4 image only has the old preview SDKs of dotnet
    $globalDotJson = Get-Content (Join-Path $PSScriptRoot '..\global.json') -Raw | ConvertFrom-Json
    $dotNetCoreSDKVersion = $globalDotJson.sdk.version
    if (-not ((dotnet --version).StartsWith($dotNetCoreSDKVersion))) {
        Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile dotnet-install.ps1
        .\dotnet-install.ps1 -Version $dotNetCoreSDKVersion
        Remove-Item .\dotnet-install.ps1
    }
}

# Implements the AppVeyor 'build_script' step
function Invoke-AppVeyorBuild {
    Param(
        [Parameter(Mandatory)]
        [ValidateSet('FullCLR', 'NetStandard')]
        $BuildType,
        
        [Parameter(Mandatory)]
        [ValidateSet('Release', 'PSv3Release')]
        $BuildConfiguration,

        [Parameter(Mandatory)]
        [ValidateScript( {Test-Path $_})]
        $CheckoutPath
    )
    
    $PSVersionTable
    Write-Verbose "Pester version: $((Get-Command Invoke-Pester).Version)" -Verbose
    Write-Verbose ".NET SDK version: $(dotnet --version)" -Verbose
    Push-Location $CheckoutPath
    if ($BuildType -eq 'FullCLR') {
        .\buildCoreClr.ps1 -Framework net451 -Configuration $BuildConfiguration -Build
    }
    elseif ($BuildType -eq 'NetStandard') {
        .\buildCoreClr.ps1 -Framework netstandard1.6 -Configuration Release -Build
    }
    .\build.ps1 -BuildDocs
    Pop-Location
}

# Implement AppVeyor 'Test_script'
function Invoke-AppVeyorTest {
    Param(
        [Parameter(Mandatory)]
        [ValidateScript( {Test-Path $_})]
        $CheckoutPath
    )

    $psScriptAnalyzerModuleOutput = (Join-Path $CheckoutPath 'out\PSScriptAnalyzer')
    if ($IsCoreCLR) {
        Copy-Item $psScriptAnalyzerModuleOutput "$env:ProgramFiles\WindowsPowerShell\Modules\" -Recurse -Force
    }
    else {
        Copy-Item $psScriptAnalyzerModuleOutput "$env:ProgramFiles\powershell\6.0.0\Modules\" -Recurse -Force
    }
    $testResultsFile = ".\TestResults.xml"
    $testScripts = (Join-Path $CheckoutPath 'Test\Engine'),(Join-Path $CheckoutPath 'Test\Rules')
    $testResults = Invoke-Pester -Script $testScripts -OutputFormat NUnitXml -OutputFile $testResultsFile -PassThru
    (New-Object 'System.Net.WebClient').UploadFile("https://ci.appveyor.com/api/testresults/nunit/${env:APPVEYOR_JOB_ID}", (Resolve-Path $testResultsFile))
    if ($testResults.FailedCount -gt 0) {
        throw "$($testResults.FailedCount) tests failed."
    }
}