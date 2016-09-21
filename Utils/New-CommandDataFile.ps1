$jsonVersion = "0.0.1"
$builtinModulePath = Join-Path $pshome 'Modules'
if (-not (Test-Path $builtinModulePath))
{
    throw new "$builtinModulePath does not exist! Cannot create command data file."
}

Function IsPSEditionDesktop
{
    $PSEdition -eq $null -or $PSEdition -eq 'Desktop'
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
    }
}
$jsonData['Modules'] = $shortModuleInfos
$jsonData | ConvertTo-Json -Depth 4 | Out-File ((Get-CmdletDataFileName))