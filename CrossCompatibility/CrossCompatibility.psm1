# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Add the relevant binary module
$compatibilityLoaded = $false
try
{
    $null = [Microsoft.PowerShell.CrossCompatibility.CompatibilityAnalysisException]
    $compatibilityLoaded = $true
}
catch
{
    # Do nothing
}
if (-not $compatibilityLoaded)
{
    if ($PSVersionTable.PSVersion.Major -ge 6)
    {
        Add-Type -LiteralPath ([System.IO.Path]::Combine($PSScriptRoot, 'netstandard2.0', 'Microsoft.PowerShell.CrossCompatibility.dll'))
    }
    else
    {
        Add-Type -LiteralPath ([System.IO.Path]::Combine($PSScriptRoot, 'net452', 'Microsoft.PowerShell.CrossCompatibility.dll'))
    }
}

# Location of directory where compatibility reports should be put
[string]$script:CompatibilityProfileDir = Join-Path $PSScriptRoot 'profiles'

# Workaround for lower PowerShell versions
[bool]$script:IsWindows = -not ($IsLinux -or $IsMacOS)

# The default parameter set name
[string]$script:DefaultParameterSet = '__AllParameterSets'

# Binding flags for static fields
[System.Reflection.BindingFlags]$script:StaticBindingFlags = 'Public,Static'

# Binding flags for instance fields -- note the 'FlattenHierarchy'
[System.Reflection.BindingFlags]$script:InstanceBindingFlags = 'Public,Instance,FlattenHierarchy'

# Common/ubiquitous cmdlet parameters which we don't want to repeat over and over
[System.Collections.Generic.HashSet[string]]$script:CommonParameters = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($p in [System.Management.Automation.Internal.CommonParameters].GetProperties())
{
    [void]$script:CommonParameters.Add($p.Name)
}

# Module path locations
if ($PSVersionTable.PSVersion.Major -ge 6)
{
    [string]$script:PSHomeModulePath = [System.Management.Automation.ModuleIntrinsics].GetMethod('GetPSHomeModulePath', [System.Reflection.BindingFlags]'static,nonpublic').Invoke($null, @())
    [string]$script:WinPSHomeModulePath = "$env:windir\System32\WindowsPowerShell\v1.0\Modules"
}
else
{
    [string]$script:PSHomeModulePath = "$PSHOME\Modules"
    [string]$script:WinPSHomeModulePath = $script:PSHomeModulePath
}

<#
.SYNOPSIS
True if the given parameter name is a common cmdlet parameter, false otherwise.

.PARAMETER ParameterName
The cmdlet parameter name to test.
#>
function Test-IsCommonParameter
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [string]
        $ParameterName
    )

    return $script:CommonParameters.Contains($ParameterName)
}

<#
.SYNOPSIS
Generate a new compatibility JSON file of the current PowerShell session
at the specified location.

.DESCRIPTION
WARNING: This command will load all modules on the module path when executed in order to build a profile.
It should be called from a fresh noprofile session, like `pwsh -NoProfile -Command { New-PowerShellCompatibilityProfile }`.
New-PowerShellCompatibilityProfile will catalog all modules available in a PowerShell session,
as well as all types available, to build a profile of that PowerShell session for later compatibility analysis.

.PARAMETER OutFile
The file location where the JSON compatibility file should be generated.
If this is null or empty, the result will be written to a file with a platform-appropriate name.

.PARAMETER PlatformName
The name to give the platform being profiled, used as the profile filename and also as the profile ID.
When this is specified, the profile is created in the current directory with the given name.
If not provided, this will be generated based off the platform data read from the machine.

.PARAMETER PassThru
If set, write the report object to output rather than to a file.

.PARAMETER PlatformId
Set a custom ID for the platform in the profile object, without setting the filename.

.PARAMETER NoCompress
If set, JSON output will include whitespace and indentation for human readability.

.PARAMETER ExcludeModulePathPrefix
Exclude modules on paths beginning with any of the given prefixes from the profile

.PARAMETER ExcludeAssemblyPathPrefix
Exclude assemblies at paths beginning with any of the given prefixes from the profile

.PARAMETER Validate
If set, validates the output before returning.
#>
function New-PowerShellCompatibilityProfile
{
    [CmdletBinding(DefaultParameterSetName='OutFile')]
    param(
        [Parameter(ParameterSetName='OutFile')]
        [string]
        $OutFile,

        [Parameter(ParameterSetName='PlatformName')]
        [ValidateNotNullOrEmpty()]
        [string]
        $PlatformName,

        [Parameter(ParameterSetName='PassThru')]
        [switch]
        $PassThru,

        [Parameter(ParameterSetName='OutFile')]
        [Parameter(ParameterSetName='PassThru')]
        [string]
        $PlatformId,

        [Parameter(ParameterSetName='OutFile')]
        [Parameter(ParameterSetName='PlatformName')]
        [switch]
        $NoCompress,

        [Parameter()]
        [string[]]
        $ExcludeModulePathPrefix,

        [Parameter()]
        [string[]]
        $ExcludeAssemblyPathPrefix,

        [switch]
        $Validate
    )

    $getArgs = @{}

    if ($ExcludeModulePathPrefix)
    {
        $getArgs.ExcludeModulePathPrefix = $ExcludeModulePathPrefix
    }

    if ($ExcludeAssemblyPathPrefix)
    {
        $getArgs.ExcludeAssemblyPathPrefix
    }

    $reportData = Get-PowerShellCompatibilityProfileData @getArgs

    if (-not $reportData)
    {
        throw "Report generation failed. See errors for more information"
    }

    if ($Validate)
    {
        try
        {
            Assert-CompatibilityProfileIsValid -CompatibilityProfile $reportData
        }
        catch
        {
            # Inform the user that a problem occurred,
            # but continue outputting data so that they can examine it
            Write-Error $_
        }
    }

    switch ($PSCmdlet.ParameterSetName)
    {
        'OutFile'
        {
            # If PlatformId
            if (-not $PlatformId)
            {
                $PlatformId = Get-PlatformName $reportData.Platform
            }

            # Deal with the output file path
            if (-not $OutFile)
            {
                # Create the profile dir if it doesn't exist
                if (-not (Test-Path $script:CompatibilityProfileDir))
                {
                    $null = New-Item -ItemType Directory $script:CompatibilityProfileDir
                }

                # The output file path is the
                $OutFile = Join-Path $script:CompatibilityProfileDir "$PlatformId.json"
            }
            elseif (-not [System.IO.Path]::IsPathRooted($OutFile))
            {
                # Otherwise,
                $OutFile = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutFile)
            }

            break
        }

        # If PlatformName is given, it becomes the ProfileId and the local file name
        'PlatformName'
        {
            $here = Get-Location
            $OutFile = Join-Path $here "$PlatformName.json"
            $PlatformId = $PlatformName
            break
        }

        # We just need the PlatformId for PassThru
        'PassThru'
        {
            if (-not $PlatformId)
            {
                $PlatformId = Get-PlatformName $reportData.Platform
            }
            break
        }
    }

    $reportData.Id = $PlatformId

    if ($PassThru)
    {
        return $reportData
    }

    ConvertTo-CompatibilityJson -Item $reportData -NoWhitespace:(-not $NoCompress) |
        Out-File -Force -LiteralPath $OutFile -Encoding Utf8

    return Get-Item -LiteralPath $OutFile
}

<#
.SYNOPSIS
Get the unique platform name of a given PowerShell platform.
#>
function Get-PlatformName
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData[]]
        $PlatformData
    )

    if (-not $PlatformData)
    {
        $PlatformData = Get-PlatformData
    }

    foreach ($platform in $PlatformData)
    {
        [Microsoft.PowerShell.CrossCompatibility.Utility.PlatformNaming]::GetPlatformName($platform)
    }
}

<#
.SYNOPSIS
Alternative to ConvertTo-Json that converts enums to strings
and does not display null fields.

PowerShell's ConvertTo-Json
serializes null fields to `"field": null` (not respecting [MemberData(EmitDefaultValue = false)])
and emits enums as integers (when we want strings). It also bases out at a certain level.

Converting compatibility JSON, we want certain serialization settings baked in.
Also, we want to fail rather than call `.ToString()` on the bottom objects.
And finally we don't want to do logic at every depth stage - we feed the whole thing to
Newtonsoft.Json, which means we can be faster.

.PARAMETER Item
The object to serialize to JSON.

.PARAMETER EnumsAsValues
If set, serializes enums as numbers rather than strings.

.PARAMETER NoWhitespace
If set, does not add any whitespace to the JSON.
#>
function ConvertTo-CompatibilityJson
{
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData]
        $Item,

        [Parameter()]
        [Alias('Compress')]
        [switch]
        $NoWhitespace
    )

    begin
    {
        if ($NoWhitespace)
        {
            $serializer = [Microsoft.PowerShell.CrossCompatibility.Utility.JsonProfileSerializer]::Create()
        }
        else
        {
            $serializer = [Microsoft.PowerShell.CrossCompatibility.Utility.JsonProfileSerializer]::Create([Newtonsoft.Json.Formatting]::Indented)
        }
    }

    process
    {
        return $serializer.Serialize($Item)
    }
}

<#
.SYNOPSIS
Converts from JSON to a compatibility profile data type.

.PARAMETER JsonSource
A string, FileInfo or TextReader object
from which to deserialize the contents.

.PARAMETER Path
Path to a file to deserialize from.
#>
function ConvertFrom-CompatibilityJson
{
    [CmdletBinding(DefaultParameterSetName='Input')]
    param(
        [Parameter(ParameterSetName='Input', Mandatory=$true, ValueFromPipeline=$true)]
        $JsonSource,

        [Parameter(ParameterSetName='File', Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path
    )

    $deserializer = [Microsoft.PowerShell.CrossCompatibility.Utility.JsonProfileSerializer]::Create()

    if ($Path)
    {
        if (-not [System.IO.Path]::IsPathRooted($Path))
        {
            $Path = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
        }

        return $deserializer.DeserializeFromFile($Path)
    }

    return $deserializer.Deserialize($JsonSource)
}

<#
.SYNOPSIS
Generate a new compatibility report object for the current PowerShell session.

.PARAMETER Runtime
The PowerShell runtime data to include in the profile.

.PARAMETER Platform
The PowerShell platform data to include in the profile.

.PARAMETER ExcludeModulePathPrefix
Exclude modules on paths beginning with any of the given prefixes from the profile

.PARAMETER ExcludeAssemblyPathPrefix
Exclude assemblies at paths beginning with any of the given prefixes from the profile
#>

function Get-PowerShellCompatibilityProfileData
{
    [CmdletBinding(DefaultParameterSetName = 'RunQuery')]
    param(
        [Parameter(ParameterSetName = 'ProvidedData')]
        [Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData]
        $Runtime,

        [Parameter(ParameterSetName = 'ProvidedData')]
        [Parameter(ParameterSetName = 'RunQuery')]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData]
        $Platform = (Get-PlatformData),

        [Parameter(ParameterSetName = 'RunQuery')]
        [string[]]
        $ExcludeModulePathPrefix,

        [Parameter(ParameterSetName = 'RunQuery')]
        [string[]]
        $ExcludeAssemblyPathPrefix
    )

    if (-not $Runtime)
    {
        $runtimeArgs = @{}
        if ($ExcludeModulePathPrefix)
        {
            $runtimeArgs.ExcludeModulePathPrefix = $ExcludeModulePathPrefix
        }

        if ($ExcludeAssemblyPathPrefix)
        {
            $runtimeArgs.ExcludeAssemblyPathPrefix = $ExcludeAssemblyPathPrefix
        }

        $Runtime = Get-PowerShellCompatibilityData @runtimeArgs
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData]@{
        Runtime = $Runtime
        Platform = $Platform
    }
}

<#
.SYNOPSIS
Get all information on the current platform running PowerShell.

.PARAMETER PowerShell
The PowerShell platform data to include.
If not set, this will be taken from the current runtime.

.PARAMETER OperatingSystem
The Operating System data to include.
If not set, this will be taken from the current runtime.

.PARAMETER DotNet
The .NET data to include.
If not set, this will be taken from the current runtime.
#>
function Get-PlatformData
{
    param(
        [Parameter()]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PowerShellData]
        $PowerShell = (Get-PowerShellRuntimeData),

        [Parameter()]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.OperatingSystemData]
        $OperatingSystem = (Get-OSData),

        [Parameter()]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.DotNetData]
        $DotNet = (Get-DotNetData)
    )

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData]@{
        PowerShell = $PowerShell
        OperatingSystem = $OperatingSystem
        DotNet = $DotNet
    }
}

<#
.SYNOPSIS
Get information about the PowerShell runtime this PowerShell session is using.
#>
function Get-PowerShellRuntimeData
{
    if ($PSVersionTable.PSVersion.Major -ge 6)
    {
        $arch = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture
    }
    elseif ([System.Environment]::Is64BitProcess)
    {
        $arch = 'X64'
    }
    else
    {
        $arch = 'X86'
    }

    $psData = @{
        Version = [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]::Create($PSVersionTable.PSVersion)
        Edition = $PSVersionTable.PSEdition
        CompatibleVersions = $PSVersionTable.PSCompatibleVersions
        RemotingProtocolVersion = $PSVersionTable.PSRemotingProtocolVersion
        SerializationVersion = $PSVersionTable.SerializationVersion
        WSManStackVersion = $PSVersionTable.WSManStackVersion
        ProcessArchitecture = $arch
    }

    if ($PSVersionTable.GitCommitId -ne $PSVersionTable.PSVersion)
    {
        $psData['GitCommitId'] = $PSVersionTable.GitCommitId
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PowerShellData]$psData
}

<#
.SYNOPSIS
Get information about the operating system this PowerShell session is using.
#>
function Get-OSData
{
    if ($script:IsWindows)
    {
        $osFamily = 'Windows'
    }
    elseif ($IsLinux)
    {
        $osFamily = 'Linux'
    }
    elseif ($IsMacOS)
    {
        $osFamily = 'MacOS'
    }
    else
    {
        $osFamily = 'Other'
    }

    if ($PSVersionTable.PSVersion.Major -ge 6)
    {
        $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
        $osName = $PSVersionTable.OS
        $osPlatform = $PSVersionTable.Platform
    }
    else
    {
        $osName = (Get-WmiObject Win32_OperatingSystem).Name.Split('|')[0]
        $osPlatform = 'Win32NT'
        if ([System.Environment]::Is64BitOperatingSystem)
        {
            $arch = 'X64'
        }
        else
        {
            $arch = 'X86'
        }
    }

    $osData = @{
        Name = $osName
        Platform = $osPlatform
        Family = $osFamily
        Architecture = $arch
    }

    if ($script:IsWindows -or $IsMacOS)
    {
        $osData['Version'] = [System.Environment]::OSVersion.Version
    }
    elseif ($IsLinux)
    {
        $osData['Version'] = uname -r
    }

    if ($script:IsWindows)
    {
        $osData['SkuId'] = Get-WindowsSkuId

        if ([System.Environment]::OSVersion.ServicePack)
        {
            $osData['ServicePack'] = [System.Environment]::OSVersion.ServicePack
        }
    }

    if ($IsLinux)
    {
        $lsbInfo = Get-LinuxLsbInfo
        if ($lsbInfo)
        {
            $osData['DistributionId'] = $lsbInfo.ID
            $osData['DistributionVersion'] = $lsbInfo.VERSION_ID
            $osData['DistributionPrettyName'] = $lsbInfo.PRETTY_NAME
        }
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.OperatingSystemData]$osData
}

<#
.SYNOPSIS
Gets the Windows SKU ID of the current OS.
#>
function Get-WindowsSkuId
{
    return (Get-CimInstance Win32_OperatingSystem).OperatingSystemSKU
}

<#
.SYNOPSIS
Get Linux platform information from the files in /etc/*-release.
#>
function Get-LinuxLsbInfo
{
    return Get-Content -Raw -Path '/etc/*-release' -ErrorAction SilentlyContinue |
        ConvertFrom-Csv -Delimiter '=' -Header 'Key','Value' |
        ForEach-Object { $acc = @{} } { $acc[$_.Key] = $_.Value } { [pscustomobject]$acc }
}

<#
.SYNOPSIS
Get information about the .NET runtime this PowerShell session is running on.
#>
function Get-DotNetData
{
    if ($IsLinux -or $IsMacOS -or $PSVersionTable.PSEdition -eq 'Core')
    {
        $runtime = 'Core'
    }
    else
    {
        $runtime = 'Framework'
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.DotNetData]@{
        Runtime = $runtime
        ClrVersion = [System.Environment]::Version
    }
}

<#
.SYNOPSIS
Get the compatibility profile of the current
PowerShell runtime.

.PARAMETER ExcludeModulePathPrefix
Exclude modules on paths beginning with any of the given prefixes from the profile

.PARAMETER ExcludeAssemblyPathPrefix
Exclude assemblies at paths beginning with any of the given prefixes from the profile
#>
function Get-PowerShellCompatibilityData
{
    param(
        [Parameter()]
        [string[]]
        $ExcludeModulePathPrefix,

        [Parameter()]
        [string[]]
        $ExcludeAssemblyPathPrefix
    )

    if ($ExcludeModulePathPrefix)
    {
        $modules = Get-AvailableModules -ExcludePathPrefix $ExcludeModulePathPrefix
    }
    else
    {
        $modules = Get-AvailableModules
    }

    if ($ExcludeAssemblyPathPrefix)
    {
        $asms = Get-AvailableTypes -ExcludeAssemblyPathPrefix $ExcludeAssemblyPathPrefix
    }
    else
    {
        $asms = Get-AvailableTypes
    }

    $typeAccelerators = Get-TypeAccelerators

    $nativeCommands = Get-Command -CommandType Application
    $aliasTable = Get-AliasTable
    $coreModule = Get-CoreModuleData

    $runtimeArgs = @{
        Modules = $modules
        AliasTable = $aliasTable
        Assemblies = $asms
        TypeAccelerators = $typeAccelerators
        NativeCommands = $nativeCommands
    }
    $compatibilityData = New-RuntimeData @runtimeArgs

    # Need to deal with semver v2 in PowerShell 6+
    if ($null -eq $PSVersionTable.PSVersion.Revision)
    {
        $psVersion = New-Object 'System.Version' $PSVersionTable.PSVersion.Major,$PSVersionTable.PSVersion.Minor,$PSVersionTable.PSVersion.Patch
    }
    else
    {
        $psVersion = $PSVersionTable.PSVersion
    }

    $coreDict = New-Object 'System.Collections.Generic.Dictionary[version, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]'
    $coreDict[$psVersion] = $coreModule

    $compatibilityData.Modules['Microsoft.PowerShell.Core'] = $coreDict

    return $compatibilityData
}

<#
.SYNOPSIS
Gets all assemblies publicly available in
the current PowerShell session.
Skips assemblies from user modules by default.

.PARAMETER ExcludeAssemblyPathPrefix
Exclude assemblies at paths beginning with any of the given prefixes from the returned data.
#>
function Get-AvailableTypes
{
    param(
        [Parameter()]
        [string[]]
        $ExcludeAssemblyPathPrefix = @($PSScriptRoot)
    )

    # In PS Core, we need to explicitly force the loading of all assemblies (which normally lazy-load)
    if ($PSEdition -eq 'Core')
    {
        Get-ChildItem $PSHOME -Filter '*.dll' | ForEach-Object { try { Add-Type -Path $_ } catch { } }
    }

    $asms = New-Object 'System.Collections.Generic.List[System.Reflection.Assembly]'

    foreach ($asm in [System.AppDomain]::CurrentDomain.GetAssemblies())
    {
        if ($asm.IsDynamic -or -not $asm.Location)
        {
            continue
        }

        # Exclude assemblies with excluded paths
        if ($ExcludeAssemblyPathPrefix -and (Test-HasAnyPrefix $asm.Location -Prefix $ExcludeAssemblyPathPrefix -IgnoreCase:$script:IsWindows))
        {
            continue
        }

        $asms.Add($asm)
    }

    return $asms
}

<#
.SYNOPSIS
Get the type accelerators in the current PowerShell session.

.DESCRIPTION
Builds a dictionary of all the type accelerators defined in the current PowerShell session.
#>
function Get-TypeAccelerators
{
    $typeAccelerators = [psobject].Assembly.GetType("System.Management.Automation.TypeAccelerators")::Get.GetEnumerator()

    $taTable = New-Object 'System.Collections.Generic.Dictionary[string, type]' ([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($taKvp in $typeAccelerators)
    {
        $taTable[$taKvp.Key] = $taKvp.Value
    }

    return $taTable
}

<#
.SYNOPSIS
Get module data about core PowerShell cmdlets.

.DESCRIPTION
Get module data about the Microsoft.PowerShell.Core pseudomodule.
#>
function Get-CoreModuleData
{
    $coreCommands = Get-Command -Module 'Microsoft.PowerShell.Core'

    $pwshExe = (Get-Process -PID $PID).Path
    $coreVariables = & $pwshExe -Command { Get-Variable } | ForEach-Object { $_.Name }
    $coreAliases = Get-Alias | Where-Object { -not $_.Module } | New-AliasData
    $coreFunctions = $coreCommands | Where-Object { $_.CommandType -eq 'Function' } | New-FunctionData
    $coreCmdlets = $coreCommands | Where-Object { $_.CommandType -eq 'Cmdlet' } | New-CmdletData

    $coreModuleData = @{}

    if ($coreVariables)
    {
        $coreModuleData['Variables'] = $coreVariables
    }

    if ($coreAliases -and $coreAliases.get_Count() -gt 0)
    {
        $coreModuleData['Aliases'] = $coreAliases
    }

    if ($coreFunctions -and $coreFunctions.get_Count() -gt 0)
    {
        $coreModuleData['Functions'] = $coreFunctions
    }

    if ($coreCmdlets -and $coreCmdlets.get_Count() -gt 0)
    {
        $coreModuleData['Cmdlets'] = $coreCmdlets
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]$coreModuleData
}

<#
.SYNOPSIS
Gets all modules available on the module path for use.

.DESCRIPTION
Gets a list of all modules that are available on the module path.
This will import all modules into a session in order to populate their aliases,
so should be invoked in a new process.
Parameters can be passed to filter out unwanted modules.

.PARAMETER ExcludePathPrefix
If found modules are on a path prefixed with one of the given paths, it will be excluded.

.PARAMETER IncludeModulesFilter
A scriptblock that returns true for modules to include and false for modules to exclude.

.EXAMPLE
# Gets all modules available on the script path
Get-AvailableModules

.EXAMPLE
# Get all modules available except those under the System32 directory
Get-AvailableModules -ExcludePathPrefix "$env:windir\System32"

.NOTES
This function will import (and remove) all modules it finds.
This may leave module DLLs loaded,
so it's recommended to run this from a new process
#>
function Get-AvailableModules
{
    [CmdletBinding(DefaultParameterSetName='ExcludePaths')]
    param(
        [Parameter(ParameterSetName='ExcludePaths')]
        [string[]]
        $ExcludePathPrefix = @($PSScriptRoot),

        [Parameter(ParameterSetName='ModuleFilter')]
        [scriptblock]
        $IncludeModulesFilter
    )

    $modsToLoad = Get-Module -ListAvailable

    # Filter out this module
    $modsToLoad = $modsToLoad | Where-Object { -not ( $_.Name -eq 'CrossCompatibility' ) }

    $mods = New-Object 'System.Collections.Generic.List[psmoduleinfo]'

    :ModuleLoop foreach ($m in $modsToLoad)
    {
        # If the module path begins with an excluded prefix, skip to the next module
        if ($ExcludePathPrefix)
        {
            foreach ($prefix in $ExcludePathPrefix)
            {
                if ($m.Path.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase))
                {
                    continue :ModuleLoop
                }
            }
        }
        elseif ($IncludeModulesFilter)
        {
            if (-not (& $IncludeModulesFilter $m))
            {
                continue
            }
        }

        try
        {
            $mi = Import-Module $m -PassThru -ErrorAction Stop
            [void]$mods.Add($mi)
        }
        catch
        {
            try
            {
                $mi = Get-ModuleInfoFromNewProcess $m
            }
            catch
            {
                # Ignore errors -- assume we just can't import the module
                Write-Warning "Ignoring module '$m' after encountering problem. Error is:`n$_"
            }
        }
        finally
        {
            $m | Remove-Module
        }
    }

    return @(,$mods)
}

<#
.SYNOPSIS
Assembles a PowerShell runtime data description object from local runtime information.

.DESCRIPTION
Collects information on .NET types, PowerShell modules, aliases,
type accelerators and native commands given into a serializable metadata format.

.PARAMETER Assemblies
A list of .NET assemblies available in the PowerShell session.

.PARAMETER Modules
A list of PowerShell modules available in the PowerShell session.

.PARAMETER AliasTable
A dictionary of aliases available in the PowerShell session.

.PARAMETER TypeAccelerators
A dictionary of type accelerators available in the PowerShell session.

.PARAMETER NativeCommands
A list of native application commands available in the PowerShell session.
#>
function New-RuntimeData
{
    param(
        [Parameter()]
        [System.Reflection.Assembly[]]
        $Assemblies,

        [Parameter()]
        [psmoduleinfo[]]
        $Modules,

        [Parameter()]
        [System.Collections.Generic.IDictionary[string, System.Management.Automation.AliasInfo[]]]
        $AliasTable,

        [Parameter()]
        [System.Collections.Generic.IDictionary[string, type]]
        $TypeAccelerators,

        [Parameter()]
        [System.Management.Automation.CommandInfo[]]
        $NativeCommands
    )

    $compatData = @{}

    if ($Modules)
    {
        $compatData.Modules = $Modules | New-ModuleData -AliasTable $AliasTable
    }

    if ($Assemblies)
    {
        $compatData.Types = New-AvailableTypeData -Assemblies $Assemblies -TypeAccelerators $TypeAccelerators
    }

    if ($NativeCommands)
    {
        $compatData.NativeCommands = $NativeCommands | New-NativeCommandData
    }

    $compatData.Common = New-CommonData -CommonParameters (Get-CommonParameters)

    return [Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData]$compatData
}

<#
.SYNOPSIS
Creates a dictionary of native command metadata object from native commands.

.DESCRIPTION
Create a serializable metadata object cataloging application commands available in a PowerShell session.

.PARAMETER InfoObject
An ApplicationInfo object describing a native application.

.EXAMPLE
Get-Command -CommandType Application | New-NativeCommandData
#>
function New-NativeCommandData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.ApplicationInfo]
        $InfoObject
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.NativeCommandData[]]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        $nativeCommandData = @{
            Path = $InfoObject.Path
        }

        if ($InfoObject.Version)
        {
            $nativeCommandData.Version = $InfoObject.Version
        }

        $nativeCommandData = [Microsoft.PowerShell.CrossCompatibility.Data.NativeCommandData]$nativeCommandData

        $existingNativeCommands = $null
        if ($dict.TryGetValue($InfoObject.Name, [ref]$existingNativeCommands))
        {
            $dict[$InfoObject.Name] = $existingNativeCommands + $nativeCommandData
            return
        }

        $dict[$InfoObject.Name] = $nativeCommandData
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Create a table of aliases available in the current PowerShell session.

.DESCRIPTION
Builds a dictionary of what aliases correspond to which commands in the current PowerShell session.
#>
function Get-AliasTable
{
    $dict = New-Object 'System.Collections.Generic.Dictionary[string, System.Management.Automation.AliasInfo[]]'
    Get-Alias | Group-Object Definition | ForEach-Object { $dict[$_.Name] = $_.Group }
    return $dict
}

<#
.SYNOPSIS
Get the cmdlet common parameters in PowerShell.

.DESCRIPTION
Gets a list of the PowerShell common parameters
implicitly provided in the CmdletBinding attribute.
#>
function Get-CommonParameters
{
    return (Get-Command Get-Command).Parameters.Values |
        Where-Object { $script:CommonParameters.Contains($_.Name) }
}

<#
.SYNOPSIS
Creates a catalog of ubiquitous PowerShell runtime features.

.DESCRIPTION
Catalogs common runtime features in PowerShell,
such as common parameters in PowerShell cmdlets.

.PARAMETER CommonParameters
A list of the common parameters available in cmdlets.
#>
function New-CommonData
{
    param(
        [Parameter()]
        [System.Management.Automation.ParameterMetadata[]]
        $CommonParameters
    )

    $params = $CommonParameters | New-ParameterData
    $aliases = $CommonParameters | New-ParameterAliasData

    return [Microsoft.PowerShell.CrossCompatibility.Data.CommonPowerShellData]@{
        Parameters = $params
        ParameterAliases = $aliases
    }
}

<#
.SYNOPSIS
Creates a table of module metadata.

.DESCRIPTION
Given a list of PSModuleInfo objects,
creates a dictionary of serializable metadata describing those modules.

.PARAMETER Module
A module to include in the table

.PARAMETER AliasTable
Global PowerShell aliases, to look up and include aliases referring to commands in this module.
#>
function New-ModuleData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [psmoduleinfo]
        $Module,

        [Parameter()]
        [System.Collections.Generic.IDictionary[string, System.Management.Automation.AliasInfo[]]]
        $AliasTable
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.JsonDictionary[version, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        $modData = @{}

        $modData['Aliases'] = $Module.ExportedAliases.Values | New-AliasData

        if ($Module.ExportedCmdlets -and $Module.ExportedCmdlets.get_Count() -gt 0)
        {
            $modData['Cmdlets'] = $Module.ExportedCmdlets.Values | New-CmdletData

            foreach ($cmdlet in $Module.ExportedCmdlets.get_Values())
            {
                $aliases = $null
                if ($AliasTable.TryGetValue($cmdlet.Name, [ref]$aliases))
                {
                    foreach ($alias in $aliases)
                    {
                        if (-not $modData['Aliases'].ContainsKey($alias.Name))
                        {
                            $null = $modData.Aliases.Add($alias.Name, $cmdlet.Name)
                        }
                    }
                }
            }
        }

        if ($Module.ExportedFunctions -and $Module.ExportedFunctions.get_Count() -gt 0)
        {
            $modData['Functions'] = $Module.ExportedFunctions.Values | New-FunctionData

            foreach ($function in $Module.ExportedFunctions.get_Values())
            {
                $aliases = $null
                if ($AliasTable.TryGetValue($function.Name, [ref]$aliases))
                {
                    foreach ($alias in $aliases)
                    {
                        if (-not $modData['Aliases'].ContainsKey($alias.Name))
                        {
                            $null = $modData.Aliases.Add($alias.Name, $cmdlet.Name)
                        }
                    }
                }
            }
        }

        if ($modData['Aliases'].get_Count() -le 0)
        {
            $modData.Remove('Aliases')
        }

        if ($Module.ExportedVariables -and $Module.ExportedVariables.get_Count() -gt 0)
        {
            $modData['Variables'] = $Module.ExportedVariables.Keys
        }

        if (-not $dict.ContainsKey($Module.Name))
        {
            $versionDict = New-Object 'System.Collections.Generic.Dictionary[version, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]'
            $dict[$Module.Name] = $versionDict
        }

        $dict[$Module.Name][$Module.Version] = [Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]$modData
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Creates a table of alias data from aliases.

.DESCRIPTION
Groups aliases given into a table of metadata for serialization.

.PARAMETER Alias
An alias to catalog.
#>
function New-AliasData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.AliasInfo]
        $Alias
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, string]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        $dict[$Alias.Name] = $Alias.Definition
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Groups cmdlets into a table for serialization.

.DESCRIPTION
Given cmdlet info objects, maps them into serializable metadata objects and aggregates them into a dictionary.

.PARAMETER Cmdlet
A cmdlet to add to the table.
#>
function New-CmdletData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.CmdletInfo]
        $Cmdlet
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.CmdletData]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        $cmdletData = @{}

        $parameterSets = $Cmdlet.ParameterSets | ForEach-Object { $_.Name } | Where-Object { $_ -ne $script:DefaultParameterSet }

        if ($parameterSets)
        {
            $cmdletData['ParameterSets'] = $parameterSets
        }

        if ($Cmdlet.OutputType)
        {
            $cmdletData['OutputType'] = $Cmdlet.OutputType | ForEach-Object {
                    if ($_.Type -as [type])
                    {
                        return Get-FullTypeName $_.Type
                    }

                    return $_.Name
                }
        }

        if ($Cmdlet.Parameters -and $Cmdlet.Parameters.get_Count() -gt 0)
        {
            $cmdletData['Parameters'] = $Cmdlet.Parameters.Values | New-ParameterData -IsCmdlet
            $parameterAliases = $Cmdlet.Parameters.Values | New-ParameterAliasData -IsCmdlet
            if ($parameterAliases -and $parameterAliases.get_Count() -gt 0)
            {
                $cmdletData['ParameterAliases'] = $parameterAliases
            }
        }

        if ($Cmdlet.DefaultParameterSet)
        {
            $cmdletData['DefaultParameterSet'] = $Cmdlet.DefaultParameterSet
        }

        $dict[$Cmdlet.Name] = [Microsoft.PowerShell.CrossCompatibility.Data.Modules.CmdletData]$cmdletData
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Aggregates runtime functions into a dictionary to serialize.

.DESCRIPTION
Collects given functions into a dictionary of serializable metadata objects for cataloging and serialization.

.PARAMETER Function
A function to add to the catalog.
#>
function New-FunctionData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.FunctionInfo]
        $Function
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.FunctionData]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        $functionData = @{
            CmdletBinding = $Function.CmdletBinding
        }

        $parameterSets = $Function.ParameterSets | ForEach-Object { $_.Name } | Where-Object { $_ -ne $script:DefaultParameterSet }
        if ($parameterSets)
        {
            $functionData['ParameterSets'] = $parameterSets
        }

        if ($Function.DefaultParameterSet)
        {
            $functionData['DefaultParameterSet'] = $Function.DefaultParameterSet
        }

        if ($Function.OutputType)
        {
            $outputTypes = $Function.OutputType | Where-Object { $_.Type } | ForEach-Object { Get-FullTypeName $_.Type }
            if ($outputTypes)
            {
                $functionData['OutputType'] = $outputTypes
            }
        }

        if ($Function.Parameters -and $Function.Parameters.get_Count() -gt 0)
        {
            $functionData['Parameters'] = $Function.Parameters.Values | New-ParameterData -IsCmdlet:$Function.CmdletBinding
            $parameterAliases = $Function.Parameters.Values | New-ParameterAliasData -IsCmdlet:$Function.CmdletBinding
            if ($parameterAliases -and $parameterAliases.get_Count() -gt 0)
            {
                $functionData['ParameterAliases'] = $parameterAliases
            }
        }

        $dict[$Function.Name] = [Microsoft.PowerShell.CrossCompatibility.Data.Modules.FunctionData]$functionData
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Aggregate command parameter aliases into a lookup dictionary.

.DESCRIPTION
Takes the parameters of a command and creates a mapping dictionary
from parameter alias names to parameter names.

.PARAMETER Parameter
A parameter whose aliases should be added to the table.

.PARAMETER IsCmdlet
Indicates whether the command whose parameters are being passed is a cmdlet.
#>
function New-ParameterAliasData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.ParameterMetadata]
        $Parameter,

        [Parameter()]
        [switch]
        $IsCmdlet
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, string]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        if ($IsCmdlet -and (Test-IsCommonParameter $Parameter.Name))
        {
            return
        }

        foreach ($alias in $Parameter.Aliases)
        {
            $dict[$alias] = $Parameter.Name
        }
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Aggregate parameters on a command into a lookup table.

.DESCRIPTION
Takes parameters from a command and maps them into
serializable metadata objects, collecting them into a dictionary by parameter name.

.PARAMETER Parameter
A parameter whose metadata to collect and include in the table.

.PARAMETER IsCmdlet
True if the command owning the parameter is a cmdlet or an advanced function, false otherwise.
#>
function New-ParameterData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.ParameterMetadata]
        $Parameter,

        [Parameter()]
        [switch]
        $IsCmdlet
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterData]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        if ($IsCmdlet -and (Test-IsCommonParameter $Parameter.Name))
        {
            return
        }

        $type = Get-FullTypeName $Parameter.ParameterType

        $parameterData = @{
            Type = $type
        }

        if ($Parameter.ParameterSets.Count -ne 1 -or -not $Parameter.ParameterSets.ContainsKey($script:DefaultParameterSet))
        {
            $parameterData['ParameterSets'] = $Parameter.ParameterSets.GetEnumerator() | New-ParameterSetData
        }

        $dict[$Parameter.Name] = [Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterData]$parameterData
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Create a table of parameter set metadata objects from given parameter set information.

.DESCRIPTION
Transforms given parameter set information objects into serializable metadata objects
and aggregates them into a dictionary keyed by parameter set name.

.PARAMETER ParameterSet
A parameter set to catalog.
#>
function New-ParameterSetData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Collections.Generic.KeyValuePair[string, System.Management.Automation.ParameterSetMetadata]]
        $ParameterSet
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterSetData]' ([System.StringComparer]::OrdinalIgnoreCase)
    }

    process
    {
        $parameterSetData = @{}

        $flags = New-Object 'System.Collections.Generic.List[Microsoft.PowerShell.CrossCompatibility.ParameterSetFlag]'

        if ($ParameterSet.Value.IsMandatory)
        {
            $flags.Add('Mandatory')
        }

        if ($ParameterSet.Value.ValueFromPipeline)
        {
            $flags.Add('ValueFromPipeline')
        }

        if ($ParameterSet.Value.ValueFromPipelineByPropertyName)
        {
            $flags.Add('ValueFromPipelineByPropertyName')
        }

        if ($ParameterSet.Value.ValueFromRemainingArguments)
        {
            $flags.Add('ValueFromRemainingArguments')
        }

        if ($ParameterSet.Value.Position -ge 0)
        {
            $parameterSetData['Position'] = $ParameterSet.Value.Position
        }

        if ($flags)
        {
            $parameterSetData['Flags'] = $flags
        }

        $dict[$ParameterSet.Key] = [Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterSetData]$parameterSetData
    }

    end
    {
        return $dict
    }
}

<#
.SYNOPSIS
Creates a catalog of types available in a PowerShell session.

.DESCRIPTION
Given assemblies and a table of type accelerators,
creates a serializable metadata object cataloging that information.

.PARAMETER Assemblies
The .NET assemblies available in the session.

.PARAMETER TypeAccelerators
The PowerShell type accelerators in the session.
#>
function New-AvailableTypeData
{
    param(
        [Parameter()]
        [System.Reflection.Assembly[]]
        $Assemblies,

        [Parameter()]
        [System.Collections.Generic.IDictionary[string, type]]
        $TypeAccelerators
    )

    if (-not $TypeAccelerators)
    {
        $TypeAccelerators = Get-TypeAccelerators
    }

    $errors = $null
    $result = [Microsoft.PowerShell.CrossCompatibility.Utility.TypeDataConversion]::AssembleAvailableTypes($Assemblies, $TypeAccelerators, [ref]$errors)

    if ($errors)
    {
        $errors | Write-Warning
    }

    return $result
}

<#
.SYNOPSIS
Get the full name of a .NET type in a PowerShell-friendly way.

.PARAMETER Type
The .NET type whose name is required.
#>
function Get-FullTypeName
{
    param(
        [Parameter(Position=0,ValueFromPipeline=$true)]
        [type]
        $Type
    )

    return [Microsoft.PowerShell.CrossCompatibility.Utility.TypeNaming]::GetFullTypeName($Type)
}

<#
.SYNOPSIS
Validate a generated compatibility profile object.

.DESCRIPTION
Checks a generated compatibility profile object for
parts that should be in it, and throws if any are not present.

.PARAMETER CompatibilityProfile
The compatibility profile to validate.
#>
function Assert-CompatibilityProfileIsValid
{
    param(
        [Parameter(Position=0,ValueFromPipeline=$true)]
        $CompatibilityProfile
    )

    $problems = New-Object 'System.Collections.Generic.List[string]'

    $platformProperties = @{
        PowerShell = @(
            "Version",
            "ProcessArchitecture"
        )
        DotNet = @(
            "Runtime",
            "ClrVersion"
        )
        OperatingSystem = @(
            "Family",
            "Name",
            "Version",
            "Architecture"
        )
    }

    foreach ($key in $platformProperties.Keys)
    {
        foreach ($subKey in $platformProperties[$key])
        {
            if (-not $CompatibilityProfile.Platform.$key.$subKey)
            {
                $problems.Add("Platform info missing: Platform.$key.$subKey")
            }
        }
    }

    if (-not $CompatibilityProfile.Runtime.Common)
    {
        $problems.Add("Common field missing")
    }
    else
    {
        if (-not $CompatibilityProfile.Runtime.Common.Parameters)
        {
            $problems.Add("Common parameters missing")
        }
        elseif (-not $CompatibilityProfile.Runtime.Common.Parameters.ContainsKey("Verbose"))
        {
            $problems.Add("Verbose common parameter missing")
        }

        if (-not $CompatibilityProfile.Runtime.Common.ParameterAliases)
        {
            $problems.Add("Common parameter aliases missing")
        }
        elseif (-not $CompatibilityProfile.Runtime.Common.ParameterAliases.ContainsKey("vb"))
        {
            $problems.Add("vb Verbose common variable alias missing")
        }
    }

    $modules = @{
        "Microsoft.PowerShell.Core" = @{
            "Get-Module" = @(
                "Name",
                "ListAvailable"
            )
            "Start-Job" = @(
                "ScriptBlock",
                "FilePath"
            )
            "Where-Object" = @(
                "FilterScript",
                "Property"
            )
        }
        "Microsoft.PowerShell.Management" = @{
            "Get-Process" = @(
                "Name",
                "Id",
                "InputObject"
            )
            "Test-Path" = @(
                "Path",
                "LiteralPath"
            )
            "Get-ChildItem" = @(
                "Path",
                "LiteralPath"
            )
            "New-Item" = @(
                "Path",
                "Name",
                "Value"
            )
        }
        "Microsoft.PowerShell.Utility" = @{
            "New-Object" = @(
                "TypeName",
                "ArgumentList"
            )
            "Write-Host" = @(
                "Object",
                "NoNewline"
            )
            "Out-File" = @(
                "FilePath",
                "Encoding",
                "Append",
                "Force"
            )
            "Invoke-Expression" = @(
                "Command"
            )
        }
    }

    if ($CompatibilityProfile.PowerShell.Version.Major -ge 4)
    {
        $modules += @{
            "PowerShellGet" = @{
                "Install-Module" = @(
                    "Name"
                    "Scope"
                )
            }
        }
    }

    if ($CompatibilityProfile.PowerShell.Version.Major -ge 5)
    {
        $modules += @{
            "PSReadLine" = @{
                "Set-PSReadLineKeyHandler" = @(
                    "Chord",
                    "ScriptBlock"
                )
                "Set-PSReadLineOption" = @(
                    "EditMode"
                    "ContinuationPrompt"
                )
            }
        }
    }

    foreach ($mKey in $modules.Keys)
    {
        $mod = $CompatibilityProfile.Runtime.Modules[$mKey]

        if (-not $mod)
        {
            $problems.Add("Missing module: $mod")
            continue
        }

        if (-not $mod.Values)
        {
            $problems.Add("No versions found for module $mKey")
            continue
        }

        $highestVersion = $mod.Keys | Sort-Object -Descending | Select-Object -First 1
        $mod = $mod[$highestVersion]

        foreach ($cKey in $modules[$mKey].Keys)
        {
            $cmdlet = $mod.Cmdlets[$cKey]

            if (-not $cmdlet)
            {
                $problems.Add("Missing cmdlet '$cKey' in module '$mKey'")
                continue
            }

            foreach ($param in $modules[$mKey][$cKey])
            {
                if (-not $cmdlet.Parameters[$param])
                {
                    $problems.Add("Missing parameter '$param' on cmdlet '$cKey' in module '$mKey'")
                }
            }
        }
    }


    $utilMod = $CompatibilityProfile.Runtime.Modules['Microsoft.PowerShell.Utility']
    $highestVersion = $utilMod.Keys | Sort-Object -Descending | Select-Object -First 1
    $utilMod = $utilMod[$highestVersion]
    foreach ($alias in 'select','fl','iwr')
    {
        if (-not $utilMod.Aliases.ContainsKey($alias))
        {
            $problems.Add("Missing alias in Microsoft.PowerShell.Utility: '$alias'")
        }
    }


    $types = @{
        "System.Management.Automation" = @{
            "System.Management.Automation" = @(
                'AliasInfo',
                'PSCmdlet',
                'PSModuleInfo',
                'SwitchParameter',
                'ProgressRecord'
            )
            "System.Management.Automation.Language" = @(
                'Parser',
                'AstVisitor',
                'ITypeName',
                'Token',
                'Ast'
            )
            "Microsoft.PowerShell" = @(
                'ExecutionPolicy'
            )
            "Microsoft.PowerShell.Commands" = @(
                'OutHostCommand',
                'GetCommandCommand',
                'GetModuleCommand',
                'InvokeCommandCommand',
                'ModuleCmdletBase'
            )
        }
        "Microsoft.PowerShell.Commands.Utility" = @{
            "Microsoft.PowerShell.Commands" = @(
                'GetDateCommand',
                'NewObjectCommand',
                'SelectObjectCommand',
                'WriteOutputCommand',
                'GroupInfo',
                'GetRandomCommand'
            )
        }
        "Microsoft.PowerShell.Commands.Management" = @{
            "Microsoft.PowerShell.Commands" = @(
                'GetContentCommand',
                'CopyItemCommand',
                'TestPathCommand',
                'GetProcessCommand',
                'SetLocationCommand',
                'WriteContentCommandBase'
            )
        }
    }

    if ($CompatibilityProfile.Platform.PowerShell.Edition -eq 'Core')
    {
        $types += @{
            'System.Private.CoreLib' = @{
                'System' = @(
                    'Object',
                    'String',
                    'Array',
                    'Type'
                )
                'System.Reflection' = @(
                    'Assembly',
                    'BindingFlags',
                    'FieldAttributes'
                )
                'System.Collections.Generic' = @(
                    'Dictionary`2',
                    'IComparer`1',
                    'List`1',
                    'IReadOnlyList`1'
                )
            }
            'System.Collections' = @{
                'System.Collections' = @(
                    'BitArray'
                )
                'System.Collections.Generic' = @(
                    'Queue`1',
                    'HashSet`1',
                    'Stack`1'
                )
            }
        }
    }
    else
    {
        $types += @{
            'mscorlib' = @{
                'System' = @(
                    'Object',
                    'String',
                    'Array',
                    'Type'
                )
                'System.Reflection' = @(
                    'Assembly',
                    'BindingFlags',
                    'FieldAttributes'
                )
                'System.Collections.Generic' = @(
                    'Dictionary`2',
                    'IComparer`1',
                    'List`1',
                    'IReadOnlyList`1'
                )
                'System.Collections' = @(
                    'BitArray'
                )
            }
        }
    }

    foreach ($asmName in $types.Keys)
    {
        $asm = $CompatibilityProfile.Runtime.Types.Assemblies[$asmName]

        if (-not $asm)
        {
            $problems.Add("Assembly not found: '$asmName'")
            continue
        }

        foreach ($namespace in $types[$asmName].Keys)
        {
            $ns = $asm.Types[$namespace]

            if (-not $ns)
            {
                $problems.Add("Namespace '$namespace' not found in assembly '$asmName'")
                continue
            }

            foreach ($typeName in $types[$asmName][$namespace])
            {
                if (-not $ns[$typeName])
                {
                    $problems.Add("Type '$typeName' not found in namespace '$namespace', assembly '$asmName'")
                }
            }
        }
    }

    $typeAccelerators = @{
        psmoduleinfo = "System.Management.Automation.PSModuleInfo"
        scriptblock = "System.Management.Automation.ScriptBlock"
        datetime = "System.DateTime"
        int = "System.Int32"
        regex = "System.Text.RegularExpressions.Regex"
        ipaddress = "System.Net.IpAddress"
    }

    foreach ($ta in $typeAccelerators.Keys)
    {
        if ($CompatibilityProfile.Runtime.Types.TypeAccelerators[$ta].Type -ne $typeAccelerators[$ta])
        {
            $problems.Add("Type accelerator '$ta' does not point to correct type")
        }
    }

    if ($CompatibilityProfile.Platform.OperatingSystem.Family -eq 'Windows')
    {
        $commands = @(
            'cmd.exe',
            'net.exe',
            'regedit.exe',
            'resmon.exe',
            'where.exe'
        )
    }
    else
    {
        $commands = @(
            'ls',
            'rm',
            'cat',
            'sh',
            'grep'
        )
    }

    foreach ($c in $commands)
    {
        $command = $CompatibilityProfile.Runtime.NativeCommands[$c]

        if (-not $command)
        {
            $problems.Add("Unable to find command '$c'")
            continue
        }

        if (-not $command.Path)
        {
            $problems.Add("No path given for command '$c'")
            continue
        }
    }

    foreach ($p in $problems)
    {
        Write-Error $p
    }

    if ($problems)
    {
        throw "Problems encountered validating profile"
    }
}

function Test-HasAnyPrefix
{
    param(
        [Parameter(Mandatory=$true)]
        [string]
        $String,

        [Parameter(Mandatory=$true)]
        [string[]]
        $Prefix,

        [Parameter()]
        [switch]
        $IgnoreCase
    )

    if ($IgnoreCase)
    {
        $strcmp = [System.StringComparison]::OrdinalIgnoreCase
    }
    else
    {
        $strcmp = [System.StringComparison]::Ordinal
    }

    foreach ($p in $Prefix)
    {
        if ($null -eq $p)
        {
            continue
        }

        if ($String.StartsWith($p, $strcmp))
        {
            return $true
        }
    }

    return $false
}

function Get-ModuleInfoFromNewProcess
{
    [CmdletBinding(DefaultParameterSetName='ModuleInfo')]
    param(
        [Parameter(ParameterSetName='ModuleInfo', Position=0, ValueFromPipeline=$true)]
        [ValidateNotNull()]
        [psmoduleinfo]
        $ModuleInfo,

        [Parameter(ParameterSetName='ModuleSpec', Position=0, ValueFromPipeline=$true)]
        [ValidateNotNull()]
        [Microsoft.PowerShell.Commands.ModuleSpecification]
        $ModuleSpecification,

        [Parameter(ParameterSetName='Path', Position=0, ValueFromPipeline=$true)]
        [ValidateNotNullOrEmpty()]
        [string]
        $Path
    )

    if ($ModuleInfo)
    {
        $modSpec = $ModuleInfo
    }
    elseif ($ModuleSpecification)
    {
        $modSpec = $ModuleSpecification
    }
    else
    {
        $modSpec = $Path
    }

    return Start-Job { Import-Module $using:modSpec -PassThru } | Wait-Job | Receive-Job
}
