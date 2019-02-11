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
    $platyPS = Get-Module -ListAvailable platyPS
    if ($null -eq $platyPS -or ($platyPS | Sort-Object Version -Descending | Select-Object -First 1).Version -lt [version]0.12)
    {
        Write-Verbose -verbose "platyPS module not found or below required version of 0.12, installing the latest version."
        Install-Module -Force -Name platyPS -Scope CurrentUser
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
        [switch]$All,

        [ValidateRange(3, 6)]
        [int]$PSVersion = $PSVersionTable.PSVersion.Major,

        [ValidateSet("Debug", "Release")]
        [string]$Configuration = "Debug",

        [switch]$Documentation
        )

    END {
        if ( $All )
        {
            # Build all the versions of the analyzer
            foreach($psVersion in 3..6) {
                Start-ScriptAnalyzerBuild -Configuration $Configuration -PSVersion $psVersion
            }
            return
        }

        $documentationFileExists = Test-Path (Join-Path $PSScriptRoot 'out\PSScriptAnalyzer\en-us\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml')
        # Build docs either when -Documentation switch is being specified or the first time in a clean repo
        if ( $Documentation -or -not $documentationFileExists )
        {
            Start-DocumentationBuild
        }

        if ($PSVersion -ge 6) {
            $framework = 'netstandard2.0'
        }
        else {
            $framework = "net452"
        }

        Push-Location -Path $projectRoot
        if (-not (Test-Path "$projectRoot/global.json"))
        {
            throw "Not in solution root"
        }

        $itemsToCopyCommon = @(
            "$projectRoot\Engine\PSScriptAnalyzer.psd1", "$projectRoot\Engine\PSScriptAnalyzer.psm1",
            "$projectRoot\Engine\ScriptAnalyzer.format.ps1xml", "$projectRoot\Engine\ScriptAnalyzer.types.ps1xml"
            )

        $destinationDir = "$projectRoot\out\PSScriptAnalyzer"
        switch ($PSVersion)
        {
            3
            {
                $destinationDirBinaries = "$destinationDir\PSv3"
            }
            4
            {
                $destinationDirBinaries = "$destinationDir\PSv4"
            }
            5
            {
                $destinationDirBinaries = "$destinationDir"
            }
            6
            {
                $destinationDirBinaries = "$destinationDir\coreclr"
            }
            default
            {
                throw "Unsupported PSVersion: '$PSVersion'"
            }
        }

        $config = "PSV${PSVersion}${Configuration}"

        # Build ScriptAnalyzer
        # The Rules project has a dependency on the Engine therefore just building the Rules project is enough
        try {
            Push-Location $projectRoot/Rules
            Write-Progress "Building ScriptAnalyzer for PSVersion '$PSVersion' using framework '$framework' and configuration '$Configuration'"
            $buildOutput = dotnet build --framework $framework --configuration "$config"
            if ( $LASTEXITCODE -ne 0 ) { throw "$buildOutput" }
        }
        catch {
            Write-Warning $_
            Write-Error "Failure to build for PSVersion '$PSVersion' using framework '$framework' and configuration '$config'"
            return
        }
        finally {
            Pop-Location
        }

        Publish-File $itemsToCopyCommon $destinationDir

        $itemsToCopyBinaries = @(
            "$projectRoot\Engine\bin\${config}\${framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
            "$projectRoot\Rules\bin\${config}\${framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll"
            )
        Publish-File $itemsToCopyBinaries $destinationDirBinaries

        $settingsFiles = Get-Childitem "$projectRoot\Engine\Settings" | ForEach-Object -MemberName FullName
        Publish-File $settingsFiles (Join-Path -Path $destinationDir -ChildPath Settings)

        if ($framework -eq 'net452') {
            Copy-Item -path "$projectRoot\Rules\bin\${config}\${framework}\Newtonsoft.Json.dll" -Destination $destinationDirBinaries
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
            $scriptBlock = [scriptblock]::Create("Invoke-Pester -Path $testScripts -OutputFormat NUnitXml -OutputFile $testResultsFile -Show Describe,Summary")
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

# BOOTSTRAPPING CODE FOR INSTALLING DOTNET
# install dotnet cli tools based on the version mentioned in global.json
function Install-Dotnet
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param ( [Parameter()][Switch]$Force )

    $json = Get-Content -raw (Join-Path $PSScriptRoot global.json) | ConvertFrom-Json
    $version = $json.sdk.Version
    if ( Test-DotnetInstallation -version $version ) {
        Write-Verbose -Verbose "dotnet version '$version' already installed"
        return
    }
    try {
        Push-Location $PSScriptRoot
        $installScriptPath = Receive-DotnetInstallScript
        If ( $PSCmdlet.ShouldProcess("$installScriptName for $version")) {
            & "${installScriptPath}" -c release -v $version
        }
    }
    catch {
        throw $_
    }
    finally {
        if ( Test-Path $installScriptPath ) {
            Remove-Item $installScriptPath
        }
        Pop-Location
    }
}

function Test-DotnetInstallation
{
    param ( $version )
    $installedVersions = dotnet --list-sdks | Foreach-Object { $_.Split()[0] }
    if ( $installedVersions -contains $version ) {
        return $true
    }
    return $false
}

function Receive-DotnetInstallScript
{
    $installScriptName = "dotnet-install.ps1"

    if ( (Test-Path Variable:IsWindows) -and -not $IsWindows ) {
        $installScriptName = "dotnet-install.sh"
    }
    $uri = "https://dot.net/v1/${installScriptName}"
    # enable Tls12 for the request
    # -SslProtocol parameter for Invoke-WebRequest wasn't in PSv3
    $securityProtocol = [System.Net.ServicePointManager]::SecurityProtocol
    $tls12 = [System.Net.SecurityProtocolType]::Tls12
    try {
        if ( ([System.Net.ServicePointManager]::SecurityProtocol -band $tls12) -eq 0 ) {
            [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor $tls12
        }
        $null = Invoke-WebRequest -Uri ${uri} -OutFile "${installScriptName}"
    }
    finally {
        [System.Net.ServicePointManager]::SecurityProtocol = $securityProtocol 
    }
    if ( (Test-Path Variable:IsWindows) -and -not $IsWindows ) {
        chmod +x $installScriptName
    }
    $installScript = Get-Item $installScriptName -ErrorAction Stop
    if ( -not $installScript ) {
        throw "Download failure of ${uri}"
    }

    return $installScript.FullName
}
