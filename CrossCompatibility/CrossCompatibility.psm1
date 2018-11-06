if ($PSVersionTable.PSVersion.Major -ge 5)
{
    Import-Module ([System.IO.Path]::Combine($PSScriptRoot, 'CrossCompatibilityBinary', 'netstandard2.0'))
}
else
{
    Import-Module ([System.IO.Path]::Combine($PSScriptRoot, 'CrossCompatibilityBinary', 'net451'))
}

$ErrorActionPreference = 'Stop'

[bool]$script:IsWindows = -not ($IsLinux -or $IsMacOS)

[string]$script:DefaultParameterSet = '__AllParameterSets'

[System.Reflection.BindingFlags]$script:StaticBindingFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static

[System.Reflection.BindingFlags]$script:InstanceBindingFlags = [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::FlattenHierarchy

[string[]]$script:SpecialMethodPrefixes = @(
    'get_'
    'set_'
    'add_'
    'remove_'
    'op_'
)

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

function New-CommonParameterSet
{
    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($p in $script:commonParams)
    {
        $set.Add($p)
    }

    return $set
}

[System.Collections.Generic.HashSet[string]]$script:CommonParameters = New-CommonParameterSet

function Test-HasSpecialPrefix
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [string]
        $MethodName)

    foreach ($prefix in $script:SpecialMethodPrefixes)
    {
        if ($MethodName.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase))
        {
            return $true
        }
    }

    return $false
}

function Test-IsCommonParameter
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [string]
        $ParameterName
    )

    return $script:CommonParameters.Contains($ParameterName)
}

function ConvertTo-TypeJson
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

function New-PowerShellCompatibilityReport
{
    param(
        [Parameter()]
        [string]
        $OutFile
    )

    if (-not $OutFile)
    {
        return Get-PowerShellCompatibilityReportData | ConvertTo-TypeJson
    }

    if (-not [System.IO.Path]::IsPathRooted($OutFile))
    {
        $here = Get-Location
        $OutFile = [System.IO.Path]::Combine($here, $OutFile)
    }

    Get-PowerShellCompatibilityReportData | ConvertTo-TypeJson > $OutFile
}

function Get-PowerShellCompatibilityReportData
{
    return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityReportData]@{
        Compatibility = Get-PowerShellCompatibilityData
        Platform = Get-PlatformData
    }
}

function Get-PlatformData
{
    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData]@{
        Machine = Get-MachineData
        PowerShell = Get-PowerShellData
        OperatingSystem = Get-OSData
        DotNet = Get-DotNetData
    }
}

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
            $osData['Distribution'] = $lsbInfo['DISTRIB_ID']
            $osData['DistributionVersion'] = $lsbInfo['VERSION_ID']
            $osData['DistributionVersionName'] = $lsbInfo['VERSION']
        }
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.Platform.OperatingSystemData]$osData
}

function Get-LinuxLsbInfo
{
    try
    {
        $fileContent = Get-Content '/etc/*-release'
    }
    catch
    {
        # If something goes wrong, just assume the file isn't there
        return $null
    }

    return $fileContent | ForEach-Object { $acc = @{} } { $k,$v = $_ -split '='; $acc[$k]=$v } { $acc }
}

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

function Get-PowerShellCompatibilityData
{
    $modules = Get-BuiltinModules
    $typeAccelerators = Get-TypeAccelerators
    $asms = Get-AvailableTypes

    $coreModule = Get-CoreModuleData

    $compatibilityData = New-CompatibilityData -Modules $modules -Assemblies $asms -TypeAccelerators $typeAccelerators

    $compatibilityData['Modules']['Microsoft.PowerShell.Core'] = $coreModule

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

    return [Microsoft.PowerShell.CrossCompatibility.Data.Module.ModuleData]$coreModuleData
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

function New-CompatibilityData
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
        $compatData['Modules'] = $Modules | New-ModuleData
    }

    if ($Assemblies)
    {
        $compatData['Types'] = New-AvailableTypeData -Assemblies $Assemblies -TypeAccelerators $TypeAccelerators
    }

    return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityData]$compatData
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
            $cmdletData['OutputType'] = $Cmdlet.OutputType | ForEach-Object { Get-FullTypeName $_.Type }
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

    return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityDataAssembler]::AssembleAvailableTypes($Assemblies, $TypeAccelerators)

    <#
    $typeDict = New-Object 'System.Collections.Generic.Dictionary[string, string]'

    if ($TypeAccelerators)
    {
        foreach ($type in $TypeAccelerators.Keys)
        {
            $typeName = Get-FullTypeName $TypeAccelerators[$type]
            [void]$typeDict.Add($type, $typeName)
        }
    }

    $asms = $Assemblies | New-AssemblyData

    return [Microsoft.PowerShell.CrossCompatibility.Data.Types.AvailableTypeData]@{
        Assemblies = $asms
        TypeAccelerators = $typeDict
    }
    #>
}

function New-AssemblyData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.Assembly]
        $Assembly
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Types.AssemblyData]'
    }

    process
    {
        $asmName = $Assembly.GetName() | New-AssemblyNameData
        $types = $Assembly.GetTypes() | Where-Object { $_.IsPublic } | New-TypeData

        $asmData = [Microsoft.PowerShell.CrossCompatibility.Data.Types.AssemblyData] @{
            AssemblyName = $asmName
            Types = $types
        }

        $dict.Add($asmName.Name, $asmData)
    }

    end
    {
        return $dict
    }
}

function New-AssemblyNameData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.AssemblyName]
        $AssemblyName
    )

    process
    {
        if ($AssemblyName.CultureName)
        {
            $culture = $AssemblyName.CultureName
        }
        else
        {
            $culture = "neutral"
        }

        $publicKeyToken = $AssemblyName.GetPublicKeyToken()

        [Microsoft.PowerShell.CrossCompatibility.Data.Types.AssemblyNameData] @{
            Name = $AssemblyName.Name
            Version = $AssemblyName.Version
            Culture = $culture
            PublicKeyToken = $publicKeyToken
        }
    }
}

function New-TypeData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [type]
        $Type
    )

    begin
    {
        $dict = New-Object 'System.Collections.Generic.Dictionary[string, System.Collections.Generic.IDictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeData]]'
    }

    process
    {
        if (-not $dict.ContainsKey($Type.Namespace))
        {
            $namespaceDict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeData]'
            $dict.Add($Type.Namespace, $namespaceDict)
        }

        $typeData = @{}

        $instanceMembers = $Type.GetMembers($script:InstanceBindingFlags)
        if ($instanceMembers)
        {
            $typeData['Instance'] = $instanceMembers | New-MemberData
        }

        $staticMembers = $Type.GetMembers($script:StaticBindingFlags)
        if ($staticMembers)
        {
            $typeData['Static'] = $staticMembers | New-MemberData
        }

        $typeData = [Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeData]$typeData

        $dict[$Type.Namespace].Add($Type.Name, $typeData)
    }

    end
    {
        return $dict
    }
}

function New-MemberData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.MemberInfo]
        $Member
    )

    begin
    {
        $memberDict = New-Object 'System.Collections.Generic.Dictionary[string, System.Reflection.MemberInfo[]]'
    }

    process
    {
        # We need to remember members we have seen to check for overriding
        if (-not $memberDict.ContainsKey($Member.Name))
        {
            $memberDict[$Member.Name] = $Member
            return
        }

        # If we see a method, check
        if ($Member.MemberType -eq [System.Reflection.MemberTypes]::Method)
        {
            $method = $Member -as [System.Reflection.MethodInfo]
            $existingMethods = $memberDict[$Member.Name]
        }
    }

    end
    {
        # Now build the actual method data
        $methodDataDict = New-Object 'System.Collections.Generic.Dictionary[string, Microsoft.PowerShell.CrossCompatibility.Data.Types.MethodData]'
        foreach ($method in $methods.GetEnumerator())
        {
            $methodData = $method | New-MethodData
            $methodDataDict.Add($method.Key, $methodData)
        }

        $memberData = @{}

        if ($constructors.get_Count() -gt 0)
        {
            $memberData['Constructors'] = $constructors
        }

        if ($events.get_Count() -gt 0)
        {
            $memberData['Events'] = $events
        }

        if ($fields.get_Count() -gt 0)
        {
            $memberData['Fields'] = $fields
        }

        if ($methodDataDict.get_Count() -gt 0)
        {
            $memberData['Methods'] = $methodDataDict
        }

        if ($nestedTypes.get_Count() -gt 0)
        {
            $memberData['NestedTypes'] = $nestedTypes
        }

        if ($properties.get_Count() -gt 0)
        {
            $memberData['Properties'] = $properties
        }

        if ($indexers.get_Count() -gt 0)
        {
            $memberData['Indexers'] = $indexers
        }

        return [Microsoft.PowerShell.CrossCompatibility.Data.Types.MemberData]$memberData
    }
}

function Get-OverridingMembers
{
    param(
        [Parameter()]
        [System.Reflection.MemberInfo]
        $NewMember,

        [Parameter()]
        [System.Reflection.MemberInfo[]]
        $CurrentMembers
    )

    if (-not $CurrentMembers)
    {
        return $NewMember
    }

    [System.Reflection.MemberTypes]$currType = $CurrentMembers[0].MemberType

    if ($CurrentMembers.Length -gt 1)
    {
        if (-not ('Property', 'Method' -contains $currType))
        {
            throw "Multiple current members of bad type: $currType"
        }

        foreach ($m in $CurrentMembers)
        {
            if ($m.MemberType -ne $currType)
            {
                throw "Multiple members of heterogenous type. Offending member: $m"
            }
        }

        switch ($currType)
        {
            Property
            {

            }
        }
    }
}

function Test-MethodMatchesParameters
{
    param(
        [Parameter()]
        [type[]]
        $GivenParamTypes,

        [Parameter()]
        [type[][]]
        $ExistingParamTypes
    )

    # If no existing methods are given, then there is no match
    if (-not $ExistingParamTypes)
    {
        return -1
    }

    # Search through each of the existing methods and find
    # if any of them has all the same parameter types as the given method.
    # If so, return the index of the exist method
    :nextmethod for ($i = 0; $i -lt $ExistingParamTypes.Length; $i++)
    {
        $paramsToMatch = $ExistingParamTypes[$i]

        if ($paramsToMatch.Length -ne $GivenParamTypes.Length)
        {
            continue nextmethod
        }

        for ($j = 0; $j -lt $GivenParamTypes.Length; $j++)
        {
            if ($GivenParamTypes[$j].ParameterType -ne $paramsToMatch[$j].ParameterType)
            {
                continue nextmethod
            }
        }

        return $i
    }

    return -1
}

function New-FieldData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.FieldInfo]
        $Field
    )

    process
    {
        return [Microsoft.PowerShell.CrossCompatibility.Data.Types.FieldData]@{
            Type = Get-FullTypeName $Field.FieldType
        }
    }
}

function New-ConstructorData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.ConstructorInfo]
        $Ctor
    )

    process
    {
        $ctorParameters = $Ctor.GetParameters()
        if ($ctorParameters)
        {
            $parameters = $Ctor.GetParameters() | ForEach-Object { Get-FullTypeName $_.ParameterType }
        }
        else
        {
            $parameters = @()
        }

        return $parameters
    }
}

function New-EventData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.EventInfo]
        $EventInfo
    )

    process
    {
        return [Microsoft.PowerShell.CrossCompatibility.Data.Types.EventData]@{
            HandlerType = Get-FullTypeName $EventInfo.EventHandlerType
            IsMulticast = $EventInfo.IsMulticast
        }
    }
}

function New-IndexerData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.PropertyInfo]
        $Indexer
    )

    process
    {
        $accessors = @()

        if ($Indexer.GetMethod -and $Indexer.GetMethod.IsPublic)
        {
            $accessors += 'Get'
        }

        if ($Indexer.SetMethod -and $Indexer.SetMethod.IsPublic)
        {
            $accessors += 'Set'
        }

        return [Microsoft.PowerShell.CrossCompatibility.Data.Types.IndexerData]@{
            ItemType = Get-FullTypeName $Indexer.PropertyType
            Parameters = $Indexer.GetIndexParameters() | ForEach-Object { Get-FullTypeName $_.ParameterType }
            Accessors = $accessors
        }
    }
}

function New-PropertyData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Reflection.PropertyInfo]
        $Property
    )

    process
    {
        $accessors = @()

        if ($Property.GetMethod -and $Property.GetMethod.IsPublic)
        {
            $accessors += 'Get'
        }

        if ($Property.SetMethod -and $Property.SetMethod.IsPublic)
        {
            $accessors += 'Set'
        }

        return [Microsoft.PowerShell.CrossCompatibility.Data.Types.PropertyData]@{
            Type = Get-FullTypeName $Property.PropertyType
            Accessors = $accessors
        }
    }
}

function New-MethodData
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [System.Collections.Generic.KeyValuePair[string, System.Collections.Generic.List[System.Reflection.MethodInfo]]]
        $Method
    )

    process
    {
        $overloads = New-Object 'System.Collections.Generic.List[string[]]'
        foreach ($methodOverload in $Method.Value)
        {
            [System.Reflection.MethodInfo]$methodOverload = $methodOverload
            $overload = $methodOverload.GetParameters() | ForEach-Object { Get-FullTypeName $_.ParameterType }
            if ($overload)
            {
                $overloads.Add($overload)
            }
            else
            {
                $overloads.Add(@())
            }
        }

        return [Microsoft.PowerShell.CrossCompatibility.Data.Types.MethodData]@{
            ReturnType = Get-FullTypeName $Method.Value[0].ReturnType
            OverloadParameters = $overloads
        }
    }
}

function Get-FullTypeName
{
    param(
        [Parameter(ValueFromPipeline=$true)]
        [type]
        $Type
    )

    process
    {
        return [Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityDataAssembler]::GetFullTypeName($Type)
    }
}