<#
.SYNOPSIS
    Create a JSON file containing modules found in $pshome and their corresponding exported commands, and available types.

.EXAMPLE
    C:\PS> ./New-CommandDataFile.ps1

    Suppose this file is run on the following version of PowerShell: PSVersion = 6.1.0-preview.2, PSEdition = Core, and Windows 10 operating system.
    Then this script will create a file named core-6.0.1-preview.2-windows.json that contains a JSON object of the following form:
    {
        "Modules" : [
            "Module1" : {
                "Name" : "Module1"
                .
                .
                "ExportedCommands" : [
                    {
                        "Name": 
                        "CommandType": 
                        "ParameterSets":
                    },
                    .
                    .
                    .
                ],
                "ExportedAliases":
            },
            .
            .
            .
        ],
        "Types" : [
            {
                "Name":
                "Namespace":
            },
            .
            .
            .
        ],
        "SchemaVersion" : "0.0.1"
    }

.INPUTS
    None

.OUTPUTS
    None
#>

$jsonVersion = "0.0.1"

# Location where libraries are to be saved.
$libraryDirectory = "<library location>"

$iotInfo = @{
    # Change to true if you want to use a remote machine to get the iot libraries
    getLibrary = $false
    # Use the correct values below for your remote machine
    ip = "<ip address>"
    user = "<user>"
    password = ConvertTo-SecureString -String "<password>" -asplaintext -force
}

$nanoInfo = @{
    # Change to true if you want to use a remote machine to get the nano libraries
    getLibrary = $false
    # Use the correct values below for your remote machine
    ip = "<ip>"
    user = "<user>"
    password = ConvertTo-SecureString -String "<password>" -asplaintext -force
    # Update the following as appropriate.
    oneCorePSDevLocation = "<....\src\onecore\admin\monad\nttargets\bin\PsOnCssScripts\OneCorePSDev\>"
    latestNanoBuildLocation = "<....\release\<latestbuild>\amd64fre\bin>"
    netTypesLocation = "<....\PowerShell\src\Microsoft.PowerShell.CoreCLR.AssemblyLoadContext\CorePsTypeCatalog.cs>"
}

function CheckOS ($osToCheck)
{
    try
    {
        $IsOS = [System.Management.Automation.Platform]::$osToCheck
        return $IsOS
    }
    catch {}
    return $false
}

$sku = [ordered]@{
    OS = 'windows'
    PowerShellEdition = $PSEdition.ToString().ToLower()
    PowerShellVersion = $PSVersionTable.PSVersion.ToString()
}

# Get OS
if (CheckOS IsLinux)
{
    $sku.OS = 'linux'
}
elseif (CheckOS IsMacOs)
{
    $sku.OS = 'macos'
}
elseif (CheckOS IsIoT)
{
    $sku.OS = 'iot'
}
elseif (CheckOS IsNanoServer)
{
    $sku.OS = 'nano'
}
# else it is windows, which is already set

Function Get-CmdletDataFileName ($PSsku)
{   
    "{0}-{1}-{2}.json" -f $PSsku.PowerShellEdition,$PSsku.PowerShellVersion,$PSsku.OS
}

[scriptblock]$retrieveCmdletScript = {

    $builtinModulePath = Join-Path $pshome 'Modules'
    if (-not (Test-Path $builtinModulePath))
    {
        throw new "$builtinModulePath does not exist! Cannot create target platform library."
    }

    $psCoreSnapinName = 'Microsoft.PowerShell.Core'
    $builtInModules = Get-Module -ListAvailable | Where-Object {$_.ModuleBase -like "${pshome}*" }
    $builtInModules += [pscustomobject]@{ Name = $psCoreSnapinName; Version = [string](get-command -module $psCoreSnapinName)[0].pssnapin.psversion }
    $moduleInfos = @()

    foreach( $module in $builtInModules )
    {
        Write-Progress $module.Name
        $moduleInfo = [ordered]@{
            Name = $module.Name
            Version = [string]$module.Version
        }
        $commands = Get-Command -Module $module.Name

        $exportedCommandsArray = @()
        foreach ($command in $commands)
        {
            $exportedCommand = [ordered]@{
                Name = $command.Name
            }
            $exportedCommandsArray += $exportedCommand
        }

        $moduleInfo['ExportedCommands'] = $exportedCommandsArray

        if ( $module.Name -eq "${psCoreSnapinName}" )
        {
            $moduleInfo['ExportedAliases'] = $commands | ForEach-Object {get-alias -ea silentlycontinue -definition $_.Name} | ForEach-Object {$_.name}
        }
        else
        {
            $moduleInfo['ExportedAliases'] = $module.ExportedAliases.Keys
        }
        $moduleInfos += [pscustomobject]$moduleInfo
    }
    return $moduleInfos
}

[scriptblock]$retrieveTypesScript = {

    param($paths)

    $types = @()
    $skippedAssemblies = @()

    foreach($path in $paths)
    {
        Set-Location $path
        $allAssemblies = Get-ChildItem *.dll

        foreach ($dll in $allAssemblies)
        {   
            $assembly = "$($dll.BaseName), Culture=neutral"
            $newAssembly=[System.Reflection.AssemblyName]::New($assembly)
            try 
            {
                $loadedAssembly = [System.Reflection.Assembly]::Load($newAssembly)
                Write-Progress "Loading assembly: $loadedAssembly"
                $types += $loadedAssembly.GetTypes() | Where-Object { $_.IsPublic } | Select-Object -Property 'Name', 'Namespace'
            }
            catch 
            {
                $skippedAssemblies += $dll
            }
        }
    }
    return $types
}

# Run on Core only 
$createTypeAcceleratorFileScript = {

    $typeAccelerators = @{}

    $typeHash = [psobject].Assembly.GetType("System.Management.Automation.TypeAccelerators")::get
    foreach ($type in $typeHash.GetEnumerator()) { $typeAccelerators.Add( ($type.Key).ToLower(), ($type.Value).fullName ) }

    # Desktop
    $typeAccelerators.Add("adsi", "System.DirectoryServices.DirectoryEntry")
    $typeAccelerators.Add("adsisearcher", "System.DirectoryServices.DirectorySearcher")
    $typeAccelerators.Add("wmiclass", "System.Management.ManagementClass")
    $typeAccelerators.Add("wmi", "System.Management.ManagementObject")
    $typeAccelerators.Add("wmisearcher", "System.Management.ManagementObjectSearcher")
     $typeAccelerators.Add("validatetrusteddata", "System.Management.Automation.ValidateTrustedDataAttribute")

    # special cases
    $typeAccelerators.Add("ordered", "System.Collections.Specialized.OrderedDictionary")
    $typeAccelerators.Add("object", "System.Object")

    # Create json file
    $typeAccelerators | ConvertTo-Json | Out-File "typeAccelerators.json" -Encoding utf8 -Force
} 


############# Core ############

if ($sku.OS -eq 'windows' -and $sku.PowerShellEdition -eq 'core')
{
    $windowsJsonData = [ordered]@{}

    $windowsJsonData.Edition = $sku
    $windowsJsonData.Modules = & $retrieveCmdletScript
    $windowsJsonData.Types = & $retrieveTypesScript $pshome
    $windowsJsonData.SchemaVersion = $jsonVersion

    Push-Location $libraryDirectory

    # set -Depth to 5 if detailed list of ParameterSets is needed.
    $windowsJsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName($sku))) -Encoding utf8 -Force

    & $createTypeAcceleratorFileScript

    Pop-Location
}

############# Linux ##################

if ($sku.OS -eq 'linux')
{
    $linuxJsonData = [ordered]@{}

    $linuxJsonData.Edition = $sku
    $linuxJsonData.Modules = & $retrieveCmdletScript
    $linuxJsonData.Types = & $retrieveTypesScript $pshome
    $linuxJsonData.SchemaVersion = $jsonVersion

    Push-Location $libraryDirectory

    # set -Depth to 5 if detailed list of ParameterSets is needed.
    $linuxJsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName($sku))) -Encoding utf8 -Force

    Pop-Location
}
############# MacOS ##################

if ($sku.OS -eq 'macos')
{
    $macosJsonData = [ordered]@{}

    $macosJsonData.Edition = $sku
    $macosJsonData.Modules = & $retrieveCmdletScript
    $macosJsonData.Types = & $retrieveTypesScript $pshome
    $macosJsonData.SchemaVersion = $jsonVersion

    Push-Location $libraryDirectory

    # set -Depth to 5 if detailed list of ParameterSets is needed.
    $macosJsonData | ConvertTo-Json -Depth 4 | Out-File (Get-CmdletDataFileName $sku) -Encoding utf8 -Force

    Pop-Location
}

############## IoT ####################

if ($iotInfo.getLibrary -or $sku.OS -eq 'iot')
{
    $credentials = New-Object -TypeName System.Management.Automation.PSCredential -argumentlist $iotInfo.user, $iotInfo.password
    $s = New-PSSession -ComputerName $iotInfo.ip -Credential $credentials

    $PSInfo = Invoke-Command -Session $s -ScriptBlock {
                $o = [PSObject]@{
                    PSVersion = $PSVersionTable.PSVersion
                    PSEdition = $PSEdition
                }
                return $o
            }

    $IoTSku = [ordered]@{
        OS = 'iot'
        PowerShellEdition = $PSInfo.PSEdition.ToString().ToLower()
        PowerShellVersion = $PSInfo.PSVersion.ToString()
    }

    $typePath = "C:\windows\system32\DotNetCore\v1.0"

    $IoTJsonData = [ordered]@{}

    $IoTJsonData.Edition = $IoTSku

    $IoTJsonData.Modules = (Invoke-Command -Session $s -ScriptBlock { 
                                param([string]$getCmdlets)
                                $sb = [scriptblock]::Create($getCmdlets)
                                [psobject]@{ output = &$sb }
                            } -ArgumentList $retrieveCmdletScript).output

    $IoTJsonData.Types = (Invoke-Command -Session $s -ScriptBlock { 
                                param([string]$getTypes, [string]$typePath)
                                $sb = [scriptblock]::Create($getTypes)
                                $allPaths = @($pshome, $typePath)
                                [psobject]@{ output = &$sb -Path $allPaths}
                            } -ArgumentList $retrieveTypesScript, $typePath).output

    $IoTJsonData.SchemaVersion = $jsonVersion

    Push-Location $libraryDirectory

    # set -Depth to 5 if detailed list of ParameterSets is needed.
    $IoTJsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName($IoTSku))) -Encoding utf8 -Force

    Pop-Location

    Remove-PSSession $s
}

############## Nano ####################

if ($nanoInfo.getLibrary -or $sku.OS -eq 'nano')
{
    $credentials = New-Object -TypeName System.Management.Automation.PSCredential -argumentlist $($nanoInfo.user), $($nanoInfo.password)

    net use \\$($nanoInfo.ip)\c$ 
    Get-ChildItem \\$($nanoInfo.ip)\c$

    Push-Location $($nanoInfo.oneCorePSDevLocation)
    Import-Module .\OneCorePSDev.psm1 -Force
    Pop-Location

    Update-OneCorePowerShell -BinaryFolder $nanoInfo.latestNanoBuildLocation -CssShare \\$($nanoInfo.ip)\c$  

    $s = New-PSSession -ComputerName $nanoInfo.ip -Credential $credentials

    $PSInfo = Invoke-Command -Session $s -ScriptBlock {
                $o = [PSObject]@{
                    PSVersion = $PSVersionTable.PSVersion
                    PSEdition = $PSEdition
                }
                return $o
            }

    $nanoSku = [ordered]@{
        OS = 'nano'
        PowerShellEdition = $PSInfo.PSEdition.ToString().ToLower()
        PowerShellVersion = $PSInfo.PSVersion.ToString()
    }

    $nanoJsonData = [ordered]@{}

    $nanoJsonData.Edition = $nanoSku

    $nanoJsonData.Modules = (Invoke-Command -Session $s -ScriptBlock { 
                            param([string]$getCmdlets)
                            $sb = [scriptblock]::Create($getCmdlets)
                            [psobject]@{ output = &$sb }
                            } -ArgumentList $retrieveCmdletScript).output

    # Get PowerShell types
    $nanoJsonData.Types = (Invoke-Command -Session $s -ScriptBlock { 
                            param([string]$getTypes)
                            $sb = [scriptblock]::Create($getTypes)
                            [psobject]@{ output = &$sb -Path $pshome}
                            } -ArgumentList $retrieveTypesScript).output

    # Get .NET types
    $lines = Get-Content $nanoInfo.netTypesLocation

    foreach ($line in $lines)
    {
        if( $line.Contains("typeCatalog["))
        {
            $newType = [ordered]@{}

            $line = $line.Split("=")[0]
            $line = $line.Replace('typeCatalog["', "").Replace('"]', "").Trim()
            $parts = $line.Split(".")
            $name = $parts[$parts.Count -1]
            $newType['Name'] = $name
            $nameSpace = ''
            for ($i = 0; $i -lt ($parts.Count -1); $i++) {
                $nameSpace += $parts[$i]
                if ($i -lt ($parts.Count - 2))
                {
                    $nameSpace += '.'
                }
            }
            $newType['Namespace'] = $nameSpace

            $nanoJsonData.Types += $newType
        }
    }

    $nanoJsonData.SchemaVersion = $jsonVersion

    Push-Location $libraryDirectory

    # set -Depth to 5 if detailed list of ParameterSets is needed.
    $nanoJsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName($nanoSku))) -Encoding utf8 -Force

    Pop-Location

    Remove-PSSession $s
}

############# Desktop PowerShell ##################

if ($sku.OS -eq 'windows' -and $sku.PowerShellEdition -eq 'desktop')
{
    ## Cmdlets ##
    $builtinModulePath = Join-Path $pshome 'Modules'
    if (-not (Test-Path $builtinModulePath))
    {
        throw new "$builtinModulePath does not exist! Cannot create target platform library."
    }

    $builtInModules = Get-Module -ListAvailable
    $moduleInfos = @()

    foreach( $module in $builtInModules )
    {
        Write-Progress "Getting cmdlets from: $($module.Name)"
        $moduleInfo = [ordered]@{
            Name = $module.Name
            Version = [string]$module.Version
        }

        $commands = Get-Command -Module $module.Name
        $exportedCommandsArray = @()

        foreach ($command in $commands)
        {
            $exportedCommand = [ordered]@{
                Name = $command.Name
                # CommandType = $command.CommandType.ToString()
                # ParameterSets = $command.ParameterSets
                # ParamAliases = (Get-Command $command).Parameters.Values | Select-Object name, aliases
            }
            $exportedCommandsArray += $exportedCommand
        }

        $moduleInfo['ExportedCommands'] = $exportedCommandsArray
        $moduleInfo['ExportedAliases'] = $module.ExportedAliases.Keys
        $moduleInfos += [pscustomobject]$moduleInfo
    }

    ## Types ##
    $paths = @("C:\Windows\Microsoft.NET\assembly\GAC_MSIL\", "C:\Windows\Microsoft.NET\assembly\GAC_64")

    $types = @()
    $failedToLoadGAC = @() #Count should be 2 (Microsoft.VisualBasic.Compatibility.Data, System.EnterpriseServices.Wrapper)

    foreach ($path in $paths)
    {
        Set-Location $path

        $directories = Get-ChildItem -Recurse -Filter *.dll | 
                       ForEach-Object {Split-Path $_.FullName -Parent} | 
                       Select-Object -Unique

        $directories | ForEach-Object { Push-Location $_ ; $dll = Get-ChildItem -Filter *.dll | 
                           Foreach-Object {
                                try
                                {
                                    Write-Progress "Loading assembly: $($_.BaseName)";
                                    $assembly = [System.Reflection.Assembly]::LoadFile($_.FullName);
                                    $types += $assembly.GetTypes() | Where-Object {$_.IsPublic} | Select-Object -Property 'Name', 'Namespace';
                                }
                                catch
                                {
                                    $failedToLoadGAC += $assembly
                                }
                           };
                           Pop-Location 
                      }
    }
    
    Set-Location $pshome

    $powerShellTypes = @()
    $assembly = ""
    $failedToLoadPS = @() #Count should be 4 (PSEvents, psluginwkr, pwrshmg, pwrship)

    $powerShellDlls = Get-ChildItem -Recurse -Filter *.dll

    foreach ($dll in $powerShellDlls)
    {   
        try 
        {
            $assembly = $dll.FullName
            Write-Progress "Loading PowerShell assembly: $assembly"
            $loadedAssembly =[System.Reflection.Assembly]::LoadFile($assembly) 
            $powerShellTypes += $loadedAssembly.GetTypes() | Where-Object { $_.IsPublic } | Select-Object -Property 'Name', 'Namespace'
        } 
        catch {
            $failedToLoadPS += $dll.Name
        }
    }
   
    $allTypes = $types + $powerShellTypes
    
    $desktopJsonData = [ordered]@{}
    $desktopJsonData.Edition = $sku
    $desktopJsonData.Modules = $moduleInfos
    $desktopJsonData.Types = $allTypes
    $desktopJsonData.SchemaVersion = $jsonVersion

    Push-Location $libraryDirectory

    # Change depth if adding parameter sets, parameter aliases, etc.
    $desktopJsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName($sku))) -Encoding utf8 -Force

    Pop-Location
}