[OutputType([System.IO.FileInfo])]
[CmdletBinding()]
param(
    [string]$OutputPath = './' # Default is current directory
)

function New-CommandDataFile {
    <#
    .SYNOPSIS
        Create a JSON file containing modules found in $PSHOME and their corresponding exported
        commands and aliases.

    .DESCRIPTION
        This script creates a JSON file containing modules found in $PSHOME and their corresponding
        exported commands and aliases. The JSON file is created in the current directory by default.
        Be sure to run this script in a clean PowerShell session to avoid picking up any aliases
        that may have been added by the user's profile script.

    .PARAMETER OutputPath
        The path where the JSON file is created. If not specified, the file is created in the
        current directory.

    .EXAMPLE
        New-CommandDataFile C:\Temp

        If you run this example in PowerShell 7.6.4 on Windows, it creates a file named
        core-7.6.4-windows.json in the C:\Temp directory.

    .INPUTS
        None

    .OUTPUTS
        System.IO.FileInfo
    #>

    [OutputType([System.IO.FileInfo])]
    [CmdletBinding()]
    param(
        [ValidateScript({ Test-Path $_ })]
        [string]$OutputPath = './' # Default is current directory
    )

    $builtinModulePath = Microsoft.PowerShell.Management\Join-Path $PSHOME 'Modules'
    if (-not (Microsoft.PowerShell.Management\Test-Path $builtinModulePath)) {
        throw "$builtinModulePath does not exist! Cannot create command data file."
    }

    ## Get the PowerShell edition, OS, and platform to create a unique file name for the JSON file

    $edition = Microsoft.PowerShell.Utility\Get-Variable -Name PSEdition -ErrorAction Ignore
    if (($edition -eq $null) -or ($edition.Value -eq 'Desktop')) {
        $edition = 'desktop'
    } else {
        $edition = 'core'
    }
    if ($IsLinux) {
        $os = 'linux'
    } elseif ($IsMacOS) {
        $os = 'macos'
    } else{
        $os = 'windows'
    }

    $joinPathSplat = @{
        Path = $OutputPath
        ChildPath = "$edition-$($PSVersionTable.PSVersion.ToString())-$os.json"
    }
    $outputFileName = Microsoft.PowerShell.Management\Join-Path @joinPathSplat

    $jsonData = @{
        SchemaVersion = '0.0.1'
        Modules       = @()
    }

    $modules = (Microsoft.PowerShell.Management\Get-ChildItem -Path $builtinModulePath -Directory).Name
    $modules += 'Microsoft.PowerShell.Core' # Add the Microsoft.PowerShell.Core snap-in to the list
    foreach ($module in $modules) {
        Microsoft.PowerShell.Utility\Write-Verbose "Processing module $module"
        $commands = Microsoft.PowerShell.Core\Get-Command -Module $module
        if (-not $commands) {
            Microsoft.PowerShell.Utility\Write-Error "No commands found for module '$module'; skipping."
            continue
        }
        $shortCommands = $commands |
            Microsoft.PowerShell.Utility\Select-Object -Property Name,
                @{Label = 'CommandType'; Expression = { $_.CommandType.ToString() } },
                @{Label = 'ParameterSets'; Expression = { $_.ParameterSets  -join ' ' } }
        <#
        Microsoft.PowerShell.Core doesn't export aliases so we can't use $module.ExportedAliases.
        Some aliases are preloaded during PowerShell startup. This code ensures that we find aliases
        for Microsoft.PowerShell.Core or any other aliases that are preloaded. This has the side
        effect of also finding aliases for commands that may have been added by the user's profile
        script. Run this command in a clean PowerShell session to avoid picking up any aliases that
        may have been added by the user's profile script.
        #>
        $aliases = Microsoft.PowerShell.Utility\Get-Alias * |
            Microsoft.PowerShell.Core\Where-Object { ($commands).Name -contains $_.ResolvedCommandName }
        if ($null -eq $aliases) {
            $aliases = @()
        } else {
            $aliases = $aliases.Name
        }

        $jsonData.Modules += [pscustomobject]@{
            Name             = $module
            Version          = $commands[0].Version.ToString()
            ExportedCommands = $shortCommands
            ExportedAliases  = $aliases
        }
    }

    $jsonData |
        Microsoft.PowerShell.Utility\ConvertTo-Json -Depth 4 |
        Microsoft.PowerShell.Utility\Out-File $outputFileName -Encoding utf8

    Microsoft.PowerShell.Management\Get-Item $outputFileName
}


New-CommandDataFile -OutputPath $OutputPath