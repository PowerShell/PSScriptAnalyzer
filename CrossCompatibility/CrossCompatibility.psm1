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
if ($compatibilityLoaded)
{
    if ($PSVersionTable.PSVersion.Major -ge 6)
    {
        Add-Type -LiteralPath ([System.IO.Path]::Combine($PSScriptRoot, 'netstandard2.0', 'CrossCompatibility.dll'))
    }
    else
    {
        Add-Type -LiteralPath ([System.IO.Path]::Combine($PSScriptRoot, 'net452', 'CrossCompatibility.dll'))
    }
}

# Location of directory where compatibility reports should be put
[string]$script:CompatibilityProfileDir = Join-Path $PSScriptRoot 'profiles'

# Workaround for lower PowerShell versions
[bool]$script:IsWindows = -not ($IsLinux -or $IsMacOS)

# The default parameter set name
[string]$script:DefaultParameterSet = '__AllParameterSets'

# Binding flags for static fields
[System.Reflection.BindingFlags]$script:StaticBindingFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static

# Binding flags for instance fields -- note the 'FlattenHierarchy'
[System.Reflection.BindingFlags]$script:InstanceBindingFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::FlattenHierarchy

# Common/ubiquitous cmdlet parameters which we don't want to repeat over and over
[System.Collections.Generic.HashSet[string]]$script:CommonParameters = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

$commonParams = @(
    'Verbose'
    'Debug'
    'ErrorAction'
    'WarningAction'
    'InformationAction'
    'ErrorVariable'
    'WarningVariable'
    'InformationVariable'
    'OutVariable'
    'OutBuffer'
    'PipelineVariable'
)

foreach ($p in $commonParams)
{
    [void]$script:CommonParameters.Add($p)
}

# The file name for the any-platform reference generated from the union of all other platforms
[string]$script:AnyPlatformUnionPlatformName = [Microsoft.PowerShell.CrossCompatibility.Utility.PlatformNaming]::AnyPlatformUnionName
[string]$script:AnyPlatformReferenceProfileFilePath = [System.IO.Path]::Combine($script:CompatibilityProfileDir, "$script:AnyPlatformUnionPlatformName.json")

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

function Join-CompatibilityProfile
{
    [CmdletBinding(DefaultParameterSetName='File')]
    param(
        [Parameter(ParameterSetName='File', Position=0, ValueFromPipeline=$true)]
        [string[]]
        $InputFile,

        [Parameter(ParameterSetName='Object', Position=0, ValueFromPipeline=$true)]
        [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData[]]
        $ProfileObject,

        [Parameter()]
        [string]
        $ProfileId
    )

    if ($PSCmdlet.ParameterSetName -eq 'File')
    {
        $profiles = New-Object 'System.Collections.Generic.List[Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData]]'

        foreach ($path in $InputFile)
        {
            $resolvedPath = Resolve-Path $path

            if (Test-Path $resolvedPath -PathType Container)
            {
                Get-ChildItem -Path $resolvedPath -Filter "*.json" `
                    | ForEach-Object { ConvertFrom-CompatibilityJson -Path $_ } `
                    | ForEach-Object { $profiles.Add($_) }

                continue
            }

            $loadedProfile = ConvertFrom-CompatibilityJson -Path $resolvedPath
            $profiles.Add($loadedProfile)
        }

        $ProfileObject = $profiles
    }

    return [Microsoft.PowerShell.CrossCompatibility.Utility.ProfileCombination]::UnionMany($ProfileId, $ProfileObject)
}

<#
.SYNOPSIS
Generate a new compatibility JSON file of the current PowerShell session
at the specified location.

.PARAMETER OutFile
The file location where the JSON compatibility file should be generated.
If this is null or empty, the result will be written to a file with a platform-appropriate name.

.PARAMETER PassThru
If set, write the report object to output.
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
        $Readable,

        [switch]
        $Validate
    )

    if ($PlatformName)
    {
        $OutFile = [System.IO.Path]::Combine($here, "$Platform.json")
        $PlatformId = $PlatformName
    }
    elseif ($OutFile -and -not [System.IO.Path]::IsPathRooted($OutFile))
    {
        $here = Get-Location
        $OutFile = [System.IO.Path]::Combine($here, $OutFile)
    }

    $reportData = Get-PowerShellCompatibilityProfileData

    if ($Validate)
    {
        Assert-CompatibilityProfileIsValid -CompatibilityProfile $reportData
    }

    if (-not $reportData)
    {
        throw "Report generation failed. Please see errors for more information"
    }

    if (-not $PlatformId)
    {
        $PlatformId = Get-PlatformName $reportData.Platform
        $reportData.Id = $PlatformId
    }

    if ($PassThru)
    {
        return $reportData
    }

    if (-not $OutFile)
    {
        if (-not (Test-Path $script:CompatibilityProfileDir))
        {
            $null = New-Item -ItemType Directory $script:CompatibilityProfileDir
        }

        $OutFile = Join-Path $script:CompatibilityProfileDir "$PlatformId.json"
    }

    ConvertTo-CompatibilityJson -Item $reportData -NoWhitespace:(-not $Readable) `
        | Out-File -Force -LiteralPath $OutFile -Encoding Utf8

    return Get-Item -LiteralPath $OutFile
}

function New-AllPlatformReferenceProfile
{
    param(
        [string]
        $Path = $script:AnyPlatformReferenceProfileFilePath,

        [string]
        $ProfileDir = $script:CompatibilityProfileDir
    )

    if (Test-Path -Path $Path)
    {
        Remove-Item -Path $Path -Force
    }

    $name = $script:AnyPlatformUnionPlatformNam

    $tmpPath = Join-Path ([System.IO.Path]::GetTempPath()) "anyprofile_union.json"

    Join-CompatibilityProfile -InputFile $ProfileDir -ProfileId $name | ConvertTo-CompatibilityJson -NoWhitespace | Out-File -Encoding UTF8 -FilePath $tmpPath

    Move-Item -Path $tmpPath -Destination $Path
}

<#
.SYNOPSIS
Get the unique platform name of a given PowerShell platform.
#>
function Get-PlatformName
{
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData[]]
        $PlatformData
    )

    foreach ($platform in $PlatformData)
    {
        [Microsoft.PowerShell.CrossCompatibility.Utility.PlatformNaming]::GetPlatformName($platform)
    }
}

<#
.SYNOPSIS
Get the unique name for the current PowerShell platform
this cmdlet is executed on.
#>
function Get-CurrentPlatformName
{
    return Get-PlatformData | Get-PlatformName
}

<#
.SYNOPSIS
Alternative to ConvertTo-Json that converts enums to strings
and does not display null fields.

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
            $Path = Join-Path (Get-Location) $Path
        }

        return $deserializer.DeserializeFromFile($Path)
    }

    return $deserializer.Deserialize($JsonSource)
}

<#
.SYNOPSIS
Generate a new compatibility report object for the current PowerShell session.
#>
function Get-PowerShellCompatibilityProfileData
{
    return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData]@{
        Runtime = Get-PowerShellCompatibilityData
        Platform = Get-PlatformData
    }
}

<#
.SYNOPSIS
Get all information on the current platform running PowerShell.
#>
function Get-PlatformData
{
    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData]@{
        PowerShell = Get-PowerShellRuntimeData
        OperatingSystem = Get-OSData
        DotNet = Get-DotNetData
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
            $osData['DistributionId'] = $lsbInfo['ID']
            $osData['DistributionVersion'] = $lsbInfo['VERSION_ID']
            $osData['DistributionPrettyName'] = $lsbInfo['PRETTY_NAME']
        }
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.OperatingSystemData]$osData
}

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
    return Get-Content -Raw -Path '/etc/*-release' -ErrorAction SilentlyContinue `
        | ConvertFrom-Csv -Delimiter '=' -Header 'Key','Value' `
        | ForEach-Object { $acc = @{} } { $acc[$_.Key] = $_.Value } { [psobject]$acc }
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
#>
function Get-PowerShellCompatibilityData
{
    $modules = Get-AvailableModules
    $typeAccelerators = Get-TypeAccelerators
    $asms = Get-AvailableTypes -All:$IncludeAllModules
    $nativeCommands = Get-Command -CommandType Application
    $aliasTable = Get-AliasTable

    $coreModule = Get-CoreModuleData

    $compatibilityData = New-RuntimeData `
        -Modules $modules `
        -AliasTable $aliasTable `
        -Assemblies $asms `
        -TypeAccelerators $typeAccelerators `
        -NativeCommands $nativeCommands

    $psVersion = New-Object 'System.Version' $PSVersionTable.PSVersion.Major,$PSVersionTable.PSVersion.Minor,$PSVersionTable.PSVersion.Patch

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

.PARAMETER All
Include
#>
function Get-AvailableTypes
{
    param(
        [Parameter()]
        [switch]
        $All
    )

    # In PS Core, we need to explicitly force the loading of all assemblies (which normally lazy-load)
    if ($PSEdition -eq 'Core')
    {
        Get-ChildItem $PSHOME -Filter '*.dll' | ForEach-Object { try { Add-Type -Path $_ } catch { } }
    }

    $asms = New-Object 'System.Collections.Generic.List[System.Reflection.Assembly]'

    $asmPaths = $PSHOME, (Split-Path $script:WinPSHomeModulePath)

    foreach ($asm in [System.AppDomain]::CurrentDomain.GetAssemblies())
    {
        if ($asm.IsDynamic -or -not $asm.Location)
        {
            continue
        }

        if ($All -or $asm.GlobalAssemblyCache -or (Test-HasAnyPrefix $asm.Location -Prefix $asmPaths -IgnoreCase:$script:IsWindows))
        {
            $asms.Add($asm)
        }
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

    $coreVariables = Get-Variable | Where-Object { -not $_.Module } | ForEach-Object { $_.Name }
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


function Get-AvailableModules
{
    $modsToLoad = Get-Module -ListAvailable

    # Filter out this module
    $modsToLoad = $modsToLoad | Where-Object { -not ( $_.Name -eq 'CrossCompatibility' ) }

    $mods = New-Object 'System.Collections.Generic.List[psmoduleinfo]'

    foreach ($m in $modsToLoad)
    {
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

function New-NativeCommandData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Management.Automation.ApplicationInfo]
        $InfoObject
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.NativeCommandData[]]' @([System.StringComparer]::OrdinalIgnoreCase)
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

        if ($dict.ContainsKey($InfoObject.Name))
        {
            $dict[$InfoObject.Name] = ($dict[$InfoObject.Name] + $nativeCommandData)
            return
        }

        $dict[$InfoObject.Name] = $nativeCommandData
    }

    end
    {
        return $dict
    }
}

function Get-AliasTable
{
    return Get-Alias `
        | ForEach-Object {
            $dict = New-Object 'System.Collections.Generic.Dictionary[string,System.Management.Automation.AliasInfo[]]'
            } {
                if ($dict.ContainsKey($_.Definition))
                {
                    $dict[$_.ReferencedCommand] += $_
                }
                else
                {
                    $dict.Add($_.Definition, @($_))
                }
            } {
                $dict
            }
}

function Get-CommonParameters
{
    return (Get-Command Get-Command).Parameters.Values `
        | Where-Object { $script:CommonParameters.Contains($_.Name) }
}

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

function Get-FullTypeName
{
    param(
        [Parameter(Position=0,ValueFromPipeline=$true)]
        [type]
        $Type
    )

    return [Microsoft.PowerShell.CrossCompatibility.Utility.TypeNaming]::GetFullTypeName($Type)
}

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