$script:RuleName = 'UseCompatibleTypes'
$script:AnyProfileConfigKey = 'AnyProfilePath'
$script:TargetProfileConfigKey = 'TargetProfilePaths'

$script:Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
$script:Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
$script:Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.134_x64_4.0.30319.42000_framework'
$script:Srv2019_6_1_profile = 'win-8_x64_10.0.17763.0_6.1.2_x64_4.0.30319.42000_core'
$script:Ubuntu1804_6_1_profile = 'ubuntu_x64_18.04_6.1.2_x64_4.0.30319.42000_core'

$script:TypeTestCases = @(
    @{ Target = $script:Srv2012_3_profile; Script = '[System.Management.Automation.ModuleIntrinsics]::GetModulePath("here", "there", "everywhere")'; Types = @('System.Management.Automation.ModuleInstrinsics'); OS = 'Windows'; ProblemCount = 1 }
)

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