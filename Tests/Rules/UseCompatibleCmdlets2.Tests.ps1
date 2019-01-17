$script:AnyProfileConfigKey = 'AnyProfilePath'
$script:TargetProfileConfigKey = 'TargetProfilePaths'

function New-CompatibleCmdletsSettings
{
    param(
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $AnyProfileUnionPath,

        [Parameter()]
        [string[]]
        $TargetProfilePaths
    )

    if (-not $TargetProfilePaths)
    {
        throw "Must specify at least one target profile"
    }

    $settingsContents = New-Object System.Text.StringBuilder

    $null = $settingsContents.Append("@{'Rules'=@{'UseCompatibleCmdlets2'=")

    $null = $settingsContents.Append('@{')
    $null = $settingsContents.Append('Enable=$true;')
    $null = $settingsContents.Append("$script:AnyProfileConfigKey='$AnyProfileUnionPath';")
    $null = $settingsContents.Append("$script:TargetProfileConfigKey=@(")

    for ($i = 0; $i -lt $TargetProfilePaths.Count - 1; $i++)
    {
        $path = $TargetProfilePaths[$i]
        $null = $settingsContents.Append("'$path',")
    }
    $path = $TargetProfilePaths[$i]
    $null = $settingsContents.Append("'$path')}")

    $null = $settingsContents.Append('}}')

    return $settingsContents.ToString()
}

$script:AssetDirPath = Join-Path $PSScriptRoot 'UseCompatibleCmdlets2'
$script:ProfileDirPath = [System.IO.Path]::Combine($PSScriptRoot, '..', '..', 'CrossCompatibility', 'profiles')
$script:AnyProfilePath = Join-Path $script:ProfileDirPath 'anyplatforms_union.json'

Describe 'UseCompatibleCmdlets2' {
    Context 'Compatible +PS6 -PS5' {
        BeforeAll {
            Import-Module ([System.IO.Path]::Combine($PSScriptRoot, '..', '..', 'out', 'PSScriptAnalyzer'))

            <#
            $settingsPath = Join-Path $TestDrive 'PS5_not_PS6.psd1'
            $targetPath = @(
                Join-Path $script:ProfileDirPath 'win-101_x64_10.0.17134.0_5.1.17134.407_x64.json'
                Join-Path $script:ProfileDirPath 'win-101_x64_10.0.17134.0_5.1.17134.407_x64.json'
            )

            New-CompatibleCmdletsSettings -AnyProfileUnionPath $script:AnyProfilePath -TargetProfilePaths $targetPath > $settingsPath
            #>

            $settings = @{
                Rules = @{
                    UseCompatibleCmdlets2 = @{
                        Enable = $true
                        TargetProfilePaths = @(
                            'win-4_x64_10.0.18312.0_5.1.18312.1000_x64'
                            'win-4_x64_10.0.18312.0_6.1.1_x64'
                            'win-4_x64_10.0.18312.0_6.2.0_x64'
                        )
                    }
                }
            }
        }

        It "Finds the correct number of problems" {
            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule 'UseCompatibleCmdlets2' -ScriptDefinition "Remove-Alias gcm" -Settings $settings

            Wait-Debugger

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].Command | Should -BeExactly 'Remove-Alias'
            $diagnostics[0].TargetPlatform.PowerShell.Version.Major | Should -Be 5
            $diagnostics[0].TargetPlatform.PowerShell.Version.Minor | Should -Be 1
        }
    }
}