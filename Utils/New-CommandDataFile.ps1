<#
.SYNOPSIS
    Create a JSON file containing module found in $pshome and their corresponding exported commands

.EXAMPLE
    C:\PS> ./New-CommandDataFile.ps1

    Suppose this file is run on the following version of PowerShell: PSVersion = 6.0.0-aplha, PSEdition = Core, and Windows 10 operating system. Then this script will create a file named core-6.0.0-alpha-windows.json that contains a JSON object of the following form:
    {
        "Modules" : [
            "Module1" : {
                "Name" : "Module1"
                .
                .
                "ExportedCommands" : {...}
            }
            .
            .
            .
        ]
        "JsonVersion" : "0.0.1"
    }

.INPUTS
    None

.OUTPUTS
    None

#>

$jsonVersion = "0.0.1"
$builtinModulePath = Join-Path $pshome 'Modules'
if (-not (Test-Path $builtinModulePath))
{
    throw new "$builtinModulePath does not exist! Cannot create command data file."
}

Function IsPSEditionDesktop
{
    $edition = Get-Variable -Name PSEdition -ErrorAction Ignore
    ($edition -eq $null) -or ($edition -eq 'Desktop')
}

Function Get-CmdletDataFileName
{
    $edition = 'core'
    $os = 'windows'
    if ((IsPSEditionDesktop))
    {
        $edition = 'desktop'
    }
    else
    {
        if ($IsLinux)
        {
            $os = 'linux'
        }
        elseif ($IsOSX)
        {
            $os = 'osx'
        }
        # else it is windows, which is already set
    }
    $sb = New-Object 'System.Text.StringBuilder'
    $sb.Append($edition) | Out-Null
    $sb.Append('-') | Out-Null
    $sb.Append($PSVersionTable.PSVersion.ToString()) | Out-Null
    $sb.Append('-') | Out-Null
    $sb.Append($os) | Out-Null
    $sb.Append('.json') | Out-Null
    $sb.ToString()
}

$jsonData = @{}
$jsonData['SchemaVersion'] = $jsonVersion
$shortModuleInfos = Get-ChildItem -Path $builtinModulePath `
| Where-Object {($_ -is [System.IO.DirectoryInfo]) -and (Get-Module $_.Name -ListAvailable)} `
| ForEach-Object {
    $modules = Get-Module $_.Name -ListAvailable
    $modules | ForEach-Object {
        $module = $_
        Write-Progress $module.Name
        $commands = Get-Command -Module $module
        $shortCommands = $commands | select -Property Name,@{Label='CommandType';Expression={$_.CommandType.ToString()}},ParameterSets
        $shortModuleInfo = $module | select -Property Name,@{Label='Version';Expression={$_.Version.ToString()}}
        Add-Member -InputObject $shortModuleInfo -NotePropertyName 'ExportedCommands' -NotePropertyValue $shortCommands -PassThru
        Add-Member -InputObject $shortModuleInfo -NotePropertyName 'ExportedAliases' -NotePropertyValue $module.ExportedAliases.Keys -PassThru
    }
}
$jsonData['Modules'] = $shortModuleInfos
$jsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName)) -Encoding utf8