# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Build module for PowerShell ScriptAnalyzer
$projectRoot = $PSScriptRoot
$analyzerName = "PSScriptAnalyzer"

function Get-AnalyzerVersion
{
    [xml]$xml = Get-Content $([io.path]::Combine($projectRoot, "Directory.Build.props"))
    $xml.Project.PropertyGroup.ModuleVersion
}

$analyzerVersion = Get-AnalyzerVersion
# location where analyzer goes
$script:destinationDir = [io.path]::Combine($projectRoot,"out","${analyzerName}", $analyzerVersion)

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

# Clean up the build location
function Remove-Build
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param ()
    END {
        if ( $PSCmdlet.ShouldProcess("${script:destinationDir}")) {
            if ( Test-Path ${script:destinationDir} ) {
                Remove-Item -Force -Recurse ${script:destinationDir}
            }
        }
    }
}

# Build documentation using platyPS
function Start-DocumentationBuild
{
    $docsPath = Join-Path $projectRoot docs
    $markdownDocsPath = Join-Path $docsPath Cmdlets
    $outputDocsPath = Join-Path $script:destinationDir en-US
    $platyPS = Get-Module -ListAvailable platyPS
    if ($null -eq $platyPS -or ($platyPS | Sort-Object Version -Descending | Select-Object -First 1).Version -lt [version]0.12)
    {
        Write-Verbose -verbose "platyPS module not found or below required version of 0.12, installing the latest version."
        Install-Module -Force -Name platyPS -Scope CurrentUser -Repository PSGallery
    }
    if (-not (Test-Path $markdownDocsPath))
    {
        throw "Cannot find markdown documentation folder."
    }
    Import-Module platyPS -Verbose:$false
    if ( -not (Test-Path $outputDocsPath)) {
        $null = New-Item -Type Directory -Path $outputDocsPath -Force
    }
    $null = New-ExternalHelp -Path $markdownDocsPath -OutputPath $outputDocsPath -Force
}

function Copy-CompatibilityProfiles
{
    if ($PSVersionTable.PSVersion.Major -le 5)
    {
        Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    }

    $profileDir = [System.IO.Path]::Combine($PSScriptRoot, 'PSCompatibilityCollector', 'profiles')
    $targetProfileDir = [io.path]::Combine($script:destinationDir,"compatibility_profiles")
    if ( -not (Test-Path $targetProfileDir) ) {
        $null = New-Item -Type Directory $targetProfileDir
    }

    Copy-Item -Force $profileDir/* $targetProfileDir
}

# build script analyzer (and optionally build everything with -All)
function Start-ScriptAnalyzerBuild
{
    [CmdletBinding(DefaultParameterSetName="BuildOne")]
    param (
        [switch]$All,

        [ValidateSet(3, 4, 5, 7)]
        [int]$PSVersion = $PSVersionTable.PSVersion.Major,

        [ValidateSet("Debug", "Release")]
        [string]$Configuration = "Debug",

        [switch]$Documentation,

        [switch]$Catalog
        )

    BEGIN {
        # don't allow the build to be started unless we have the proper Cli version
        if ( -not (Test-SuitableDotnet) ) {
            $requiredVersion = $script:wantedVersion
            $foundVersion = Get-InstalledCLIVersion
            Write-Warning "No suitable dotnet CLI found, requires version '$requiredVersion' found only '$foundVersion'"
        }
        $verboseWanted = $false
        if ( $PSBoundParameters['Verbose'] ) {
            $verboseWanted = $PSBoundParameters['Verbose'].ToBool()
        }
    }
    END {

        # Build docs either when -Documentation switch is being specified or the first time in a clean repo
        $documentationFileExists = Test-Path (Join-Path $PSScriptRoot 'out\PSScriptAnalyzer\en-us\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml')
        if ( $Documentation -or -not $documentationFileExists )
        {
            Write-Verbose -Verbose:$verboseWanted -Message "Start-DocumentationBuild"
            Start-DocumentationBuild -Verbose:$verboseWanted
        }

        if ( $All )
        {
            # Build all the versions of the analyzer
            foreach ($psVersion in 3, 4, 5, 7) {
                Write-Verbose -Verbose -Message "Configuration: $Configuration PSVersion: $psVersion"
                Start-ScriptAnalyzerBuild -Configuration $Configuration -PSVersion $psVersion -Verbose:$verboseWanted
            }
            if ( $Catalog ) {
                New-Catalog -Location $script:destinationDir
            }
            return
        }

        if (-not $profilesCopied)
        {
            Write-Verbose -Verbose:$verboseWanted -Message "Copy-CompatibilityProfiles"
            Copy-CompatibilityProfiles
            # Set the variable in the caller's scope, so this will only happen once
            Set-Variable -Name profilesCopied -Value $true -Scope 1
        }

        $framework = 'net462'
        if ($PSVersion -eq 7) {
            $framework = 'net6'
        }

        # build the appropriate assembly
        if ($PSVersion -match "[34]" -and $Framework -ne "net462")
        {
            throw ("ScriptAnalyzer for PS version '{0}' is not applicable to {1} framework" -f $PSVersion,$Framework)
        }

        Push-Location -Path $projectRoot
        if (-not (Test-Path "$projectRoot/global.json"))
        {
            throw "Not in solution root"
        }

        # "Copy" the module file with the version placeholder replaced
        $manifestContent = Get-Content -LiteralPath "$projectRoot\Engine\PSScriptAnalyzer.psd1" -Raw
        $newManifestContent = $manifestContent -replace '{{ModuleVersion}}', $analyzerVersion
        Set-Content -LiteralPath "$script:destinationDir\PSScriptAnalyzer.psd1" -Encoding utf8 -Value $newManifestContent

        $itemsToCopyCommon = @(
            "$projectRoot\LICENSE",
            "$projectRoot\README.md",
            "$projectRoot\SECURITY.md",
            "$projectRoot\ThirdPartyNotices.txt",
            "$projectRoot\Engine\PSScriptAnalyzer.psm1",
            "$projectRoot\Engine\ScriptAnalyzer.format.ps1xml",
            "$projectRoot\Engine\ScriptAnalyzer.types.ps1xml"
            )

        switch ($PSVersion)
        {
            3
            {
                $destinationDirBinaries = "$script:destinationDir\PSv3"
            }
            4
            {
                $destinationDirBinaries = "$script:destinationDir\PSv4"
            }
            5
            {
                $destinationDirBinaries = "$script:destinationDir"
            }
            7
            {
                $destinationDirBinaries = "$script:destinationDir\PSv7"
            }
            default
            {
                throw "Unsupported PSVersion: '$PSVersion'"
            }
        }

        $buildConfiguration = $Configuration
        if ((3, 4, 7) -contains $PSVersion) {
            $buildConfiguration = "PSV${PSVersion}${Configuration}"
        }

        # Build ScriptAnalyzer
        # The Rules project has a dependency on the Engine therefore just building the Rules project is enough
        try {
            Push-Location $projectRoot/Rules
            $message = "Building ScriptAnalyzer for PSVersion '$PSVersion' using framework '$framework' and configuration '$Configuration'"
            Write-Verbose -Verbose:$verboseWanted -Message "$message"
            Write-Progress "$message"
            if ( -not $script:DotnetExe ) {
                $script:DotnetExe = Get-DotnetExe
            }
            $dotnetArgs = "build",
                "--framework",
                $framework,
                "--configuration",
                "$buildConfiguration"
            if ( $env:TF_BUILD ) {
                $dotnetArgs += "--output"
                $dotnetArgs += "${PSScriptRoot}\bin\${buildConfiguration}\${framework}"
            }
            $buildOutput = & $script:DotnetExe $dotnetArgs 2>&1
            if ( $LASTEXITCODE -ne 0 ) {
                Write-Verbose -Verbose -Message "dotnet is $(${script:DotnetExe}.Source)"
                $dotnetArgs | Foreach-Object {"dotnetArg: $_"} | Write-Verbose -Verbose
                Get-PSCallStack | Write-Verbose -Verbose
                throw $buildOutput
            }
            Write-Verbose -Verbose:$verboseWanted -message "$buildOutput"
        }
        catch {
            $_.TargetObject | Write-Warning
            Write-Error "Failure to build for PSVersion '$PSVersion' using framework '$framework' and configuration '$buildConfiguration'"
            throw
        }
        finally {
            Pop-Location
        }

        Publish-File $itemsToCopyCommon $script:destinationDir

        if ( $env:TF_BUILD ) {
            $itemsToCopyBinaries = @(
                "$projectRoot\bin\${buildConfiguration}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
                "$projectRoot\bin\${buildConfiguration}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll"
                "$projectRoot\bin\${buildConfiguration}\${framework}\Microsoft.PowerShell.CrossCompatibility.dll"
                )
        }
        else {
            $itemsToCopyBinaries = @(
                "$projectRoot\Engine\bin\${buildConfiguration}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
                "$projectRoot\Rules\bin\${buildConfiguration}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll"
                "$projectRoot\Rules\bin\${buildConfiguration}\${framework}\Microsoft.PowerShell.CrossCompatibility.dll"
                )
        }
        if ($Configuration -eq 'Debug') {
            $itemsToCopyBinaries += @(
                "$projectRoot\Engine\bin\${buildConfiguration}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.pdb",
                "$projectRoot\Rules\bin\${buildConfiguration}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.pdb"
                "$projectRoot\Rules\bin\${buildConfiguration}\${framework}\Microsoft.PowerShell.CrossCompatibility.pdb"
            )
        }
        Publish-File $itemsToCopyBinaries $destinationDirBinaries

        $settingsFiles = Get-Childitem "$projectRoot\Engine\Settings" | ForEach-Object -MemberName FullName
        Publish-File $settingsFiles (Join-Path -Path $script:destinationDir -ChildPath Settings)

        $rulesProjectOutputDir = if ($env:TF_BUILD) {
            "$projectRoot\bin\${buildConfiguration}\${framework}" }
        else {
            "$projectRoot\Rules\bin\${buildConfiguration}\${framework}"
        }
        if ($framework -eq 'net462') {
            $nsoft = Join-Path $rulesProjectOutputDir 'Newtonsoft.Json.dll'
            Copy-Item -path $nsoft -Destination $destinationDirBinaries
        }
        else {
            Copy-Item -Path (Join-Path $rulesProjectOutputDir 'Pluralize.NET.dll') -Destination $destinationDirBinaries
        }

        Pop-Location
    }
}

function New-Catalog
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleCommands', '')]
    param ( [Parameter()]$Location )
    $newFileCatalog = Get-Command -ErrorAction SilentlyContinue New-FileCatalog
    if ($null -eq $newFileCatalog) {
        Write-Warning "New-FileCatalog not found, not creating catalog"
        return
    }
    try {
        Push-Location $Location
        New-FileCatalog -CatalogFilePath PSScriptAnalyzer.cat -Path .
    }
    finally {
        Pop-Location
    }
}

# TEST HELPERS
# Run our tests
function Test-ScriptAnalyzer
{
    [CmdletBinding()]
    param ( [switch] $InProcess )

    END {
        # versions 3 and 4 don't understand versioned module paths, so we need to rename the directory of the version to
        # the module name, and then set the ModulePath to that
        #
        # the layout of the build location is
        # .../out
        #        /PSScriptAnalyzer
        #                         /1.18.0
        #                                /<modulefiles live here>
        # and ".../out" is added to env:PSModulePath
        # on v3 and v4, it will be
        # .../out
        #        /PSScriptAnalyzer
        #                         /PSScriptAnalyzer
        #                                          /<modulefiles live here>
        # and ".../out/PSScriptAnalyzer" is added to env:PSModulePath
        #
        #
        $major = $PSVersionTable.PSVersion.Major
        if ( $major -lt 5 ) {
            # get the directory name of the destination, we need to change it
            $versionDirectoryRoot = Split-Path $script:destinationDir
            $testModulePath = Join-Path $versionDirectoryRoot $analyzerName
        }
        else {
            $testModulePath = Join-Path "${projectRoot}" -ChildPath out
        }
        $testScripts = "'${projectRoot}\Tests\Build','${projectRoot}\Tests\Engine','${projectRoot}\Tests\Rules','${projectRoot}\Tests\Documentation'"
        try {
            if ( $major -lt 5 ) {
                Rename-Item $script:destinationDir ${testModulePath}
            }
            $savedModulePath = $env:PSModulePath
            $env:PSModulePath = "${testModulePath}{0}${env:PSModulePath}" -f [System.IO.Path]::PathSeparator
            $analyzerPsd1Path = Join-Path -Path $script:destinationDir -ChildPath "$analyzerName.psd1"
            $scriptBlock = [scriptblock]::Create("Import-Module '$analyzerPsd1Path'; Invoke-Pester -Path $testScripts -CI")
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
            if ( $major -lt 5 ) {
                Rename-Item ${testModulePath} ${script:destinationDir}
            }
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
    param ( $logfile = (Join-Path -Path ${projectRoot} -ChildPath testResults.xml) )
    $logPath = (Resolve-Path $logfile).Path
    $results = [xml](Get-Content $logPath)
    $results.SelectNodes(".//test-case[@result='Failure']")
}

function Get-GlobalJsonSdkVersion {
    param ( [switch]$Raw )
    $json = Get-Content -raw (Join-Path $PSScriptRoot global.json) | ConvertFrom-Json
    $version = $json.sdk.Version
    if ( $Raw ) {
        return $version
    }
    else {
        ConvertTo-PortableVersion $version
    }
}

# we don't have semantic version in earlier versions of PowerShell, so we need to
# create something that we can use
function ConvertTo-PortableVersion {
    param ( [string[]]$strVersion )
    if ( -not $strVersion ) {
        return (ConvertTo-PortableVersion "0.0.0-0")
    }
    foreach ( $v in $strVersion ) {
        $ver, $pre = $v.split("-",2)
        try {
            [int]$major, [int]$minor, [int]$patch, $unused = $ver.Split(".",4)
            if ( -not $pre ) {
                $pre = $unused
            }
        }
        catch {
            Write-Warning "Cannot convert '$v' to portable version"
            continue
        }
        $h = @{
            Major = $major
            Minor = $minor
            Patch = $patch
        }
        if ( $pre ) {
            $h['PrereleaseLabel'] = $pre
        }
        else {
            $h['PrereleaseLabel'] = [String]::Empty
        }
        $customObject = [pscustomobject]$h
        # we do this so we can get an approximate sort, since this implements a pseudo-version
        # type in script, we need a way to find the highest version of dotnet, it's not a great solution
        # but it will work in most cases.
        Add-Member -inputobject $customObject -Type ScriptMethod -Name ToString -Force -Value {
            $str = "{0:0000}.{1:0000}.{2:0000}" -f $this.Major,$this.Minor,$this.Patch
            if ( $this.PrereleaseLabel ) {
                $str += "-{0}" -f $this.PrereleaseLabel
            }
            return $str
        }
        Add-Member -inputobject $customObject -Type ScriptMethod -Name IsContainedIn -Value {
            param ( [object[]]$collection )
            foreach ( $object in $collection ) {
                if (
                    $this.Major -eq $object.Major -and
                    $this.Minor -eq $object.Minor -and
                    $this.Patch -eq $object.Patch -and
                    $this.PrereleaseLabel -eq $object.PrereleaseLabel
                    ) {
                    return $true
                }
            }
            return $false
        }
        $customObject
    }
}

# see https://learn.microsoft.com/dotnet/core/tools/global-json for rules
# on how version checks are done
function Test-SuitableDotnet {
    param (
        $availableVersions = $( Get-InstalledCliVersion),
        $requiredVersion = $script:wantedVersion
        )

    if ( $requiredVersion -is [String] -or $requiredVersion -is [Version] ) {
        $requiredVersion = ConvertTo-PortableVersion "$requiredVersion"
    }

    $availableVersionList = $availableVersions | ForEach-Object { if ( $_ -is [string] -or $_ -is [version] ) { ConvertTo-PortableVersion $_ } else { $_ } }
    $availableVersions = $availableVersionList
    # if we have what was requested, we can use it
    if ( $RequiredVersion.IsContainedIn($availableVersions)) {
        return $true
    }
    # if we had found a match, we would have returned $true above
    # exact match required for 2.1.100 through 2.1.201
    if ( $RequiredVersion.Major -eq 2 -and $RequiredVersion.Minor -eq 1 -and $RequiredVersion.Patch -ge 100 -and $RequiredVersion.Patch -le 201 ) {
        return $false
    }
    # we need to check each available version for something that's useable
    foreach ( $version in $availableVersions ) {
        # major/minor numbers don't match - keep looking
        if ( $version.Major -ne $requiredVersion.Major -or $version.Minor -ne $requiredVersion.Minor ) {
            continue
        }
        $requiredPatch = $requiredVersion.Patch
        $possiblePatch = $version.Patch

        if ( $requiredPatch -gt $possiblePatch ) {
            continue
        }
        if ( [math]::Abs(($requiredPatch - $possiblePatch)) -lt 100 ) {
            return $true
        }
    }
    return $false
}

# these are mockable functions for testing
function Get-InstalledCLIVersion {
    # dotnet might not have been installed _ever_, so just return 0.0.0.0
    if ( -not $script:DotnetExe ) {
        Write-Warning "Dotnet executable not found"
        return (ConvertTo-PortableVersion 0.0.0)
    }
    try {
        # earlier versions of dotnet do not support --list-sdks, so we'll check the output
        # and use dotnet --version as a fallback
        $sdkList = & $script:DotnetExe --list-sdks 2>&1
        if ( $sdkList -match "Unknown option" ) {
            $installedVersions = & $script:DotnetExe --version 2>$null
        }
        else {
            $installedVersions = $sdkList | Foreach-Object { $_.Split()[0] }
        }
    }
    catch {
        Write-Verbose -Verbose "$_"
        $installedVersions = & $script:DotnetExe --version 2>$null
    }
    return (ConvertTo-PortableVersion $installedVersions)
}

function Get-DotnetExe
{
    param ( $version = $script:wantedVersion )
    $discoveredDotnet = Get-Command -CommandType Application dotnet -ErrorAction SilentlyContinue -All
    if ( $discoveredDotnet ) {
        # it's possible that there are multiples. Take the highest version we find
        # the problem is that invoking dotnet on a version which is lower than the specified
        # version in global.json will produce an error, so we can only take the dotnet which executes
        #
        # dotnet --version has changed its output, so we have to work much harder to determine what's installed.
        # dotnet --version can now emit a list of installed sdks as output *and* an error if the global.json
        # file points to a version of the sdk which is *not* installed. However, the format of the new list
        # with --version has a couple of spaces at the beginning of the line, so we need to be resilient
        # against that.
        $properDotnet = $discoveredDotNet |
            Where-Object {
                & $_ --list-sdks |
                    Where-Object {
                        $_ -match $version
                    }
            } |
            Select-Object -Last 1
        if ( $properDotnet ) {
            $script:DotnetExe = $properDotnet
            return $properDotnet
        }
    }
    # it's not in the path, try harder to find it by checking some usual places
    if ( ! (test-path variable:IsWindows) -or $IsWindows ) {
        $dotnetHuntPath = "$HOME\AppData\Local\Microsoft\dotnet\dotnet.exe"
        Write-Verbose -Verbose "checking Windows $dotnetHuntPath"
        if ( test-path $dotnetHuntPath ) {
            $script:DotnetExe = $dotnetHuntPath
            return $dotnetHuntPath
        }
    }
    else {
        $dotnetHuntPath = "$HOME/.dotnet/dotnet"
        Write-Verbose -Verbose "checking non-Windows $dotnetHuntPath"
        if ( test-path $dotnetHuntPath ) {
            $script:DotnetExe = $dotnetHuntPath
            return $dotnetHuntPath
        }
    }

    Write-Warning "Could not find dotnet executable"
    return [String]::Empty
}

try {
    # The version we want based on the global.JSON file
    # suck this before getting the dotnet exe
    $script:wantedVersion = Get-GlobalJsonSdkVersion -Raw
    $script:DotnetExe = Get-DotnetExe
}
catch {
    Write-Warning "The dotnet CLI was not found, please install it: https://aka.ms/dotnet-cli"
}

# Copies the built PSCompatibilityCollector module to the output destination for PSSA
function Copy-CrossCompatibilityModule
{
    param(
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $Destination
    )

    $destInfo = Get-Item -Path $Destination -ErrorAction SilentlyContinue

    # Can't copy to a file
    if ($destInfo -and -not $destInfo.PSIsContainer)
    {
        throw "Destination exists but is not a directory"
    }

    # Create the destination if it does not exist
    if (-not $destInfo)
    {
        New-Item -Path $Destination -ItemType Directory
    }

    $compatCollectorModuleName = 'PSCompatibilityCollector'

    $outputAssets = @(
        "$PSScriptRoot/$compatCollectorModuleName/$compatCollectorModuleName.psd1"
        "$PSScriptRoot/$compatCollectorModuleName/$compatCollectorModuleName.psm1"
        "$PSScriptRoot/$compatCollectorModuleName/CrossCompatibilityBinary"
        "$PSScriptRoot/$compatCollectorModuleName/profiles"
    )

    foreach ($assetPath in $outputAssets)
    {
        try
        {
            Copy-Item -Path $assetPath -Destination $Destination -Recurse -Force -ErrorAction Stop
        }
        catch
        {
            # Display the problem as a warning, but continue
            Write-Warning $_
        }
    }
}

# creates the nuget package which can be used for publishing to the gallery
function Start-CreatePackage
{
    try {
        $buildRoot = "out"
        $repoName = [guid]::NewGuid().ToString()
        $nupkgDir = Join-Path $PSScriptRoot $buildRoot
        $null = Register-PSRepository -Name $repoName -InstallationPolicy Trusted -SourceLocation $nupkgDir
        Push-Location $nupkgDir

        Publish-Module -Path $PWD/PSScriptAnalyzer -Repository $repoName
    }
    finally {
       Pop-Location
       Unregister-PSRepository -Name $repoName
    }
}
