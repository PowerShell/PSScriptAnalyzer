# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

$ErrorActionPreference = 'Stop'

# Implements the AppVeyor 'install' step and installs the required versions of Pester, platyPS and the .Net Core SDK if needed.
function Invoke-AppVeyorInstall {
    $requiredPesterVersion = '4.3.1'
    $pester = Get-Module Pester -ListAvailable | Where-Object { $_.Version -eq $requiredPesterVersion }
    if ($null -eq $pester) {
        if ($null -eq (Get-Module -ListAvailable PowershellGet)) {
            # WMF 4 image build
            nuget install Pester -Version $requiredPesterVersion -source https://www.powershellgallery.com/api/v2 -outputDirectory "$env:ProgramFiles\WindowsPowerShell\Modules\." -ExcludeVersion
        }
        else {
            # Visual Studio 2017 build (has already Pester v3, therefore a different installation mechanism is needed to make it also use the new version 4)
            Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
        }
    }

    if ($null -eq (Get-Module -ListAvailable PowershellGet)) {
        # WMF 4 image build
        nuget install platyPS -Version 0.9.0 -source https://www.powershellgallery.com/api/v2 -outputDirectory "$Env:ProgramFiles\WindowsPowerShell\Modules\." -ExcludeVersion
    }
    else {
        Install-Module -Name platyPS -Force -Scope CurrentUser -RequiredVersion '0.9.0'
    }

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
    [Environment]::SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", 1) # avoid unneccessary initialization in CI
    if ($BuildType -eq 'FullCLR') {
        .\buildCoreClr.ps1 -Framework net451 -Configuration $BuildConfiguration -Build
    }
    elseif ($BuildType -eq 'NetStandard') {
        .\buildCoreClr.ps1 -Framework netstandard1.6 -Configuration Release -Build
    }
    .\build.ps1 -BuildDocs
    Pop-Location
}

# Implements AppVeyor 'on_finish' step
function Invoke-AppveyorFinish {
    $stagingDirectory = (Resolve-Path ..).Path
    $zipFile = Join-Path $stagingDirectory "$(Split-Path $pwd -Leaf).zip"
    Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    [System.IO.Compression.ZipFile]::CreateFromDirectory($pwd, $zipFile)
    @(
        # You can add other artifacts here
        (Get-ChildItem $zipFile)
    ) | ForEach-Object { Push-AppveyorArtifact $_.FullName }
}