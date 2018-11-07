# Add the relevant binary module
if ($PSVersionTable.PSVersion.Major -ge 5)
{
    Add-Type -LiteralPath ([System.IO.Path]::Combine($PSScriptRoot, 'CrossCompatibilityBinary', 'netstandard2.0', 'CrossCompatibility.dll'))
}
else
{
    Add-Type -LiteralPath ([System.IO.Path]::Combine($PSScriptRoot, 'CrossCompatibilityBinary', 'net451', 'CrossCompatibility.dll'))
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
[string[]]$script:commonParams = @(
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

<#
.SYNOPSIS
Turn the common parameters into a hashset for faster matching.
#>
function New-CommonParameterSet
{
    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($p in $script:commonParams)
    {
        $set.Add($p)
    }

    return $set
}

# Set of the common cmdlet parameters to exclude from cmdlet data
[System.Collections.Generic.HashSet[string]]$script:CommonParameters = New-CommonParameterSet

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

        [Parameter(ParameterSetName='PassThru')]
        [switch]
        $PassThru
    )

    if ($PassThru)
    {
        return Get-PowerShellCompatibilityProfileData | ConvertTo-CompatibilityJson
    }

    if ($OutFile -and -not [System.IO.Path]::IsPathRooted($OutFile))
    {
        $here = Get-Location
        $OutFile = [System.IO.Path]::Combine($here, $OutFile)
    }

    $reportData = Get-PowerShellCompatibilityProfileData

    if (-not $OutFile)
    {
        if (-not (Test-Path $script:CompatibilityProfileDir))
        {
            $null = New-Item -ItemType Directory $script:CompatibilityProfileDir
        }

        $platformName = Get-PlatformName $reportData.Platform
        $OutFile = Join-Path $script:CompatibilityProfileDir "$platformName.json"
    }

    $reportData | ConvertTo-CompatibilityJson | Out-File -FilePath $OutFile -Force
}

function Get-PlatformName
{
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData]
        $PlatformData
    )

    [Microsoft.PowerShell.CrossCompatibility.Utility.PlatformNaming]::GetPlatformName($PlatformData)
}

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
        [Parameter(ValueFromPipeline=$true)]
        [object]
        $Item,

        [Parameter()]
        [switch]
        $EnumsAsValues=$false,

        [Parameter()]
        [Alias('Compress')]
        [switch]
        $NoWhitespace=$false
    )

    begin
    {
        $settings = New-Object Newtonsoft.Json.JsonSerializerSettings

        $versionConverter = New-Object Newtonsoft.Json.Converters.VersionConverter
        $settings.Converters.Add($versionConverter)

        if (-not $EnumsAsValues)
        {
            $enumConverter = New-Object Newtonsoft.Json.Converters.StringEnumConverter
            $settings.Converters.Add($enumConverter)
        }

        if (-not $NoWhitespace)
        {
            $settings.Formatting = [Newtonsoft.Json.Formatting]::Indented
        }
    }

    process
    {
        return [Newtonsoft.Json.JsonConvert]::SerializeObject($Item, $settings)
    }
}

<#
.SYNOPSIS
Generate a new compatibility report object for the current PowerShell session.
#>
function Get-PowerShellCompatibilityProfileData
{
    return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData]@{
        Compatibility = Get-PowerShellRuntimeData
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
        Machine = Get-MachineData
        PowerShell = Get-PowerShellData
        OperatingSystem = Get-OSData
        DotNet = Get-DotNetData
    }
}

<#
.SYNOPSIS
Get information about the machine this PowerShell session is running on.
#>
function Get-MachineData
{
    if ([System.Environment]::Is64BitProcess)
    {
        $bits = 64
    }
    else
    {
        $bits = 32
    }

    if ($script:IsWindows)
    {
        $arch = $env:PROCESSOR_ARCHITECTURE
    }
    else
    {
        $arch = uname -m
        if ($arch -eq 'x86_64')
        {
            $arch = 'AMD64'
        }
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.MachineData]@{
        Bitness = $bits
        Architecture = $arch
    }
}

<#
.SYNOPSIS
Get information about the PowerShell runtime this PowerShell session is using.
#>
function Get-PowerShellData
{
    $psData = @{
        Version = $PSVersionTable.PSVersion
        Edition = $PSVersionTable.PSEdition
        CompatibleVersions = $PSVersionTable.PSCompatibleVersions
        RemotingProtocolVersion = $PSVersionTable.PSRemotingProtocolVersion
        SerializationVersion = $PSVersionTable.SerializationVersion
        WSManStackVersion = $PSVersionTable.WSManStackVersion
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

    $osData = @{
        Name = $PSVersionTable.OS
        Platform = $PSVersionTable.Platform
        Family = $osFamily
    }

    if ($script:IsWindows -or $IsMacOS)
    {
        $osData['Version'] = [System.Environment]::OSVersion.Version
    }
    elseif ($IsLinux)
    {
        $osData['Version'] = uname -r
    }

    if ($script:IsWindows -and [System.Environment]::OSVersion.ServicePack)
    {
        $osData['ServicePack'] = [System.Environment]::OSVersion.ServicePack
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

function Get-PowerShellRuntimeData
{
    $modules = Get-BuiltinModules
    $typeAccelerators = Get-TypeAccelerators
    $asms = Get-AvailableTypes

    $coreModule = Get-CoreModuleData

    $compatibilityData = New-RuntimeData -Modules $modules -Assemblies $asms -TypeAccelerators $typeAccelerators

    $compatibilityData.Modules['Microsoft.PowerShell.Core'] = $coreModule

    return $compatibilityData
}

function Get-AvailableTypes
{
    $asms = New-Object 'System.Collections.Generic.List[System.Reflection.Assembly]'
    foreach ($asm in [System.AppDomain]::CurrentDomain.GetAssemblies())
    {
        [System.Reflection.Assembly]$asm = $asm
        if ($asm.IsDynamic)
        {
            continue
        }

        # We only want assemblies that are in the GAC or come shipped with PowerShell
        if ($asm.GlobalAssemblyCache)
        {
            $asms.Add($asm)
            continue
        }

        if ($asm.Location -and ($asm.Location.StartsWith($PSHOME)))
        {
            $asms.Add($asm)
            continue
        }
    }

    return $asms
}

function Get-TypeAccelerators
{
    $typeAccelerators = [psobject].Assembly.GetType("System.Management.Automation.TypeAccelerators")::Get.GetEnumerator()

    $taTable = New-Object 'System.Collections.Generic.Dictionary[string, type]'

    foreach ($taKvp in $typeAccelerators)
    {
        $taTable[$taKvp.Key] = $taKvp.Value
    }

    return $taTable
}

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

function Get-BuiltinModules
{
    $modMatch = [regex]::Escape($PSHOME)

    if ($script:IsWindows)
    {
        $windowsModulePath = [regex]::Escape("$env:windir\System32\WindowsPowerShell\v1.0\Modules\")
        $modMatch = "^($modMatch|$windowsModulePath)"
    }

    $modsToLoad = Get-Module -ListAvailable | Where-Object { $_.Path -match $modMatch }

    $mods = New-Object 'System.Collections.Generic.List[psmoduleinfo]'

    foreach ($m in $modsToLoad)
    {
        try
        {
            $mi = Import-Module $m -PassThru
            [void]$mods.Add($mi)
        }
        catch
        {
            # Ignore errors -- assume we just can't import the module
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
        [System.Collections.Generic.IDictionary[string, type]]
        $TypeAccelerators
    )

    $compatData = @{}

    if ($Modules)
    {
        $compatData.Modules = $Modules | New-ModuleData
    }

    if ($Assemblies)
    {
        $compatData.Types = New-AvailableTypeData -Assemblies $Assemblies -TypeAccelerators $TypeAccelerators
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData]$compatData
}

function New-ModuleData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [psmoduleinfo]
        $Module
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]'
    }

    process
    {
        $modData = @{}

        if ($Module.ExportedAliases -and $Module.ExportedAliases.get_Count() -gt 0)
        {
            $modData['Aliases'] = $Module.ExportedAliases.Values | New-AliasData
        }

        if ($Module.ExportedCmdlets -and $Module.ExportedCmdlets.get_Count() -gt 0)
        {
            $modData['Cmdlets'] = $Module.ExportedCmdlets.Values | New-CmdletData
        }

        if ($Module.ExportedFunctions -and $Module.ExportedFunctions.get_Count() -gt 0)
        {
            $modData['Functions'] = $Module.ExportedFunctions.Values | New-FunctionData
        }

        if ($Module.ExportedVariables -and $Module.ExportedVariables.get_Count() -gt 0)
        {
            $modData['Variables'] = $Module.ExportedVariables.Keys
        }

        $dict[$Module.Name] = [Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData]$modData
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
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, string]'
    }

    process
    {
        $dict[$Alias.Name] = $Alias.ReferencedCommand.Name
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
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.CmdletData]'
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
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.FunctionData]'
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
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, string]'
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
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterData]'
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
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterSetData]'
    }

    process
    {
        $parameterSetData = @{}

        $flags = New-Object 'System.Collections.Generic.List[Microsoft.PowerShell.CrossCompatibility.Data.Modules.ParameterSetFlag]'

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

    return [Microsoft.PowerShell.CrossCompatibility.Utility.RuntimeDataAssembler]::AssembleAvailableTypes($Assemblies, $TypeAccelerators)
}

function Get-FullTypeName
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [type]
        $Type
    )

    return [Microsoft.PowerShell.CrossCompatibility.Utility.RuntimeDataAssembler]::GetFullTypeName($Type)
}