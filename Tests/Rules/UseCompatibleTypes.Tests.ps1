$script:RuleName = 'UseCompatibleTypes'
$script:AnyProfileConfigKey = 'AnyProfilePath'
$script:TargetProfileConfigKey = 'TargetProfilePaths'

Describe 'UseCompatibleTypes' {
    Context 'Targeting a single profile' {
        It "Reports <ProblemCount> problem(s) with <Script> on <OS> with PowerShell <Version> targeting <Target>" -TestCases $script:TypeCompatibilityTestCases {
            param($Script, [string]$Target, [string[]]$Types, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    $script:RuleName = @{
                        Enable = $true
                        TargetProfilePaths = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $script:RuleName -ScriptDefinition $Script -Settings $settings

            $diagnostics.Count | Should -Be $ProblemCount

            for ($i = 0; $i -lt $diagnostics.Count; $i++)
            {
                $diagnostics[$i].Type | Should -BeExactly $Types[$i]
                $diagnostics[$i].TargetPlatform.OperatingSystem.Family | Should -Be $OS
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Major | Should -Be $Version.Major
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Minor | Should -Be $Version.Minor
            }
        }
    }
}