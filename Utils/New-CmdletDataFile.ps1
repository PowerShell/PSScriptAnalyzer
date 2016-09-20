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

# Cannot use the ExportedCommands property of moduleinfo object because the parametersets field in an ExportedCommands element is empty
Get-ChildItem -Path $builtinModulePath `
| Where-Object {$_ -is [System.IO.DirectoryInfo]} `
| ForEach-Object {Write-Progress $_; Get-Module $_.Name -ListAvailable} `
| select -Property Name,@{Label='Version';Expression={$_.Version.ToString()}},@{Label='ExportedCommands';Expression={Get-Command -Module $_ | select Name,@{Label='CommandType';Expression={$_.CommandType.ToString()}},ParameterSets}} `
| ConvertTo-Json -Depth 4 `
| Out-File ((Get-CmdletDataFileName))