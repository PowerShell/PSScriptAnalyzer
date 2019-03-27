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
    if ( $IsCoreCLR -and -not $IsWindows )
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
        if ((Test-Path $ModulePath) -and (Get-Item $ModulePath).PSIsContainer )
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

    $profileDir = [System.IO.Path]::Combine($PSScriptRoot, 'PSCompatibilityAnalyzer', 'profiles')
    $destinationDir = [System.IO.Path]::Combine($PSScriptRoot, 'out', 'PSScriptAnalyzer', "compatibility_profiles")
    if ( -not (Test-Path $destinationDir) ) {
        $null = New-Item -Type Directory $destinationDir
    }

    Copy-Item -Force $profileDir/* $destinationDir
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

    BEGIN {
        # don't allow the build to be started unless we have the proper Cli version
        # this will not actually install dotnet if it's already present, but it will
        # install the proper version
        Install-Dotnet
        if ( -not (Test-SuitableDotnet) ) {
            $requiredVersion = Get-GlobalJsonSdkVersion
            $foundVersion = Get-InstalledCLIVersion
            Write-Warning "No suitable dotnet CLI found, requires version '$requiredVersion' found only '$foundVersion'"
        }
    }
    END {

        # Build docs either when -Documentation switch is being specified or the first time in a clean repo
        $documentationFileExists = Test-Path (Join-Path $PSScriptRoot 'out\PSScriptAnalyzer\en-us\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml')
        if ( $Documentation -or -not $documentationFileExists )
        {
            Start-DocumentationBuild
        }

        # Destination for the composed module when built
        $destinationDir = "$projectRoot\out\PSScriptAnalyzer"

        if ( $All )
        {
            # Build all the versions of the analyzer
            foreach($psVersion in 3..6) {
                Start-ScriptAnalyzerBuild -Configuration $Configuration -PSVersion $psVersion
            }
            return
        }

        if (-not $profilesCopied)
        {
            Copy-CompatibilityProfiles
            # Set the variable in the caller's scope, so this will only happen once
            Set-Variable -Name profilesCopied -Value $true -Scope 1
        }

        if ($PSVersion -ge 6) {
            $framework = 'netstandard2.0'
        }
        else {
            $framework = "net452"
        }

        # build the appropriate assembly
        if ($PSVersion -match "[34]" -and $Framework -eq "core")
        {
            throw ("ScriptAnalyzer for PS version '{0}' is not applicable to {1} framework" -f $PSVersion,$Framework)
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
            if ( -not $script:DotnetExe ) {
                $script:DotnetExe = Get-DotnetExe
            }
            $buildOutput = & $script:DotnetExe build --framework $framework --configuration "$config" 2>&1
            if ( $LASTEXITCODE -ne 0 ) { throw "$buildOutput" }
        }
        catch {
            Write-Warning $_
            Write-Error "Failure to build for PSVersion '$PSVersion' using framework '$framework' and configuration '$config'"
            throw
        }
        finally {
            Pop-Location
        }

        Publish-File $itemsToCopyCommon $destinationDir

        $itemsToCopyBinaries = @(
            "$projectRoot\Engine\bin\${config}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
            "$projectRoot\Rules\bin\${config}\${Framework}\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll"
            "$projectRoot\Rules\bin\${config}\${framework}\Microsoft.PowerShell.CrossCompatibility.dll"
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
    param ( [Parameter()][switch]$InProcess, [switch]$ShowAll )

    END {
        $testModulePath = Join-Path "${projectRoot}" -ChildPath out
        $testResultsFile = "'$(Join-Path ${projectRoot} -childPath TestResults.xml)'"
        $testScripts = "'${projectRoot}\Tests\Engine','${projectRoot}\Tests\Rules','${projectRoot}\Tests\Documentation'"
        try {
            $savedModulePath = $env:PSModulePath
            $env:PSModulePath = "${testModulePath}{0}${env:PSModulePath}" -f [System.IO.Path]::PathSeparator
            if ($ShowAll)
            {
                $scriptBlock = [scriptblock]::Create("Invoke-Pester -Path $testScripts -OutputFormat NUnitXml -OutputFile $testResultsFile")
            }
            else
            {
                $scriptBlock = [scriptblock]::Create("Invoke-Pester -Path $testScripts -OutputFormat NUnitXml -OutputFile $testResultsFile -Show Describe,Summary")
            }
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
    param (
        [Parameter()][Switch]$Force,
        [Parameter()]$version = $( Get-GlobalJsonSdkVersion -Raw )
        )

    if ( Test-DotnetInstallation -requestedversion $version ) {
        if ( $Force ) {
            Write-Verbose -Verbose "Installing again"
        }
        else {
            return
        }
    }

    try {
        Push-Location $PSScriptRoot
        $installScriptPath = Receive-DotnetInstallScript
        $installScriptName = [System.IO.Path]::GetFileName($installScriptPath)
        If ( $PSCmdlet.ShouldProcess("$installScriptName for $version")) {
            & "${installScriptPath}" -c release -version $version
        }
        # this may be the first time that dotnet has been installed,
        # set up the executable variable
        if ( -not $script:DotnetExe ) {
            $script:DotnetExe = Get-DotnetExe
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

# see https://docs.microsoft.com/en-us/dotnet/core/tools/global-json for rules
# on how version checks are done
function Test-SuitableDotnet {
    param (
        $availableVersions = $( Get-InstalledCliVersion),
        $requiredVersion = $( Get-GlobalJsonSdkVersion )
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
        $sdkList = "Unknown option"
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

function Test-DotnetInstallation
{
    param (
        $requestedVersion = $( Get-GlobalJsonSdkVersion ),
        $installedVersions = $( Get-InstalledCLIVersion )
        )
    return (Test-SuitableDotnet -availableVersions $installedVersions -requiredVersion $requestedVersion )
}

function Receive-File {
    param ( [Parameter(Mandatory,Position=0)]$uri )

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
    return $installScript
}

function Receive-DotnetInstallScript
{
    # param '$platform' is a hook to enable forcing download of a specific
    # install script, generally it should not be used except in testing.
    param ( $platform = "" )

    # if $platform has been set, it has priority
    # if it's not set to Windows or NonWindows, it will be ignored
    if ( $platform -eq "Windows" ) {
        $installScriptName = "dotnet-install.ps1"
    }
    elseif ( $platform -eq "NonWindows" ) {
        $installScriptName = "dotnet-install.sh"
    }
    elseif ( ((Test-Path Variable:IsWindows) -and -not $IsWindows) ) {
        # if the variable IsWindows exists and it is set to false
        $installScriptName = "dotnet-install.sh"
    }
    else { # the default case - we're running on a Windows system
        $installScriptName = "dotnet-install.ps1"
    }
    $uri = "https://dot.net/v1/${installScriptName}"

    $installScript = Receive-File -Uri $uri
    return $installScript.FullName
}

function Get-DotnetExe
{
    $discoveredDotnet = Get-Command -CommandType Application dotnet -ErrorAction SilentlyContinu
    if ( $discoveredDotnet ) {
        # it's possible that there are multiples. Take the highest version we find
        # the problem is that invoking dotnet on a version which is lower than the specified
        # version in global.json will produce an error, so we can only take the dotnet which executes
        $latestDotnet = $discoveredDotNet |
            Where-Object { try { & $_ --version 2>$null } catch { } } |
            Sort-Object { $pv = ConvertTo-PortableVersion (& $_ --version 2>$null); "$pv" } |
            Select-Object -Last 1
        if ( $latestDotnet ) {
            $script:DotnetExe = $latestDotnet
            return $latestDotnet
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
$script:DotnetExe = Get-DotnetExe

# Copies the built PSCompatibilityAnalyzer module to the output destination for PSSA
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

    $outputAssets = @(
        "$PSScriptRoot/PSCompatibilityAnalyzer/PSCompatibilityAnalyzer.psd1"
        "$PSScriptRoot/PSCompatibilityAnalyzer/PSCompatibilityAnalyzer.psm1"
        "$PSScriptRoot/PSCompatibilityAnalyzer/CrossCompatibilityBinary"
        "$PSScriptRoot/PSCompatibilityAnalyzer/profiles"
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
