# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Import-Module "$PSScriptRoot/../../out/PSScriptAnalyzer"

$script:RuleName = 'UseCompatibleTypes'
$script:AnyProfileConfigKey = 'AnyProfilePath'
$script:TargetProfileConfigKey = 'TargetProfilePaths'

$script:Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
$script:Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
$script:Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.134_x64_4.0.30319.42000_framework'
$script:Srv2019_6_1_profile = 'win-8_x64_10.0.17763.0_6.1.2_x64_4.0.30319.42000_core'
$script:Ubuntu1804_6_1_profile = 'ubuntu_x64_18.04_6.1.2_x64_4.0.30319.42000_core'

$script:TypeCompatibilityTestCases = @(
    @{ Target = $script:Srv2012_3_profile; Script = '[System.Management.Automation.ModuleIntrinsics]::GetModulePath("here", "there", "everywhere")'; Types = @('System.Management.Automation.ModuleIntrinsics'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '$ast -is [System.Management.Automation.Language.FunctionMemberAst]'; Types = @('System.Management.Automation.Language.FunctionMemberAst'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '$version = [System.Management.Automation.SemanticVersion]::Parse($version)'; Types = @('System.Management.Automation.SemanticVersion'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '$kw = New-Object "System.Management.Automation.Language.DynamicKeyword"'; Types = @('System.Management.Automation.Language.DynamicKeyword'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '& { param([Parameter(Position=0)][ArgumentCompleter({"Banana"})][string]$Hello) $Hello } "Banana"'; Types = @('ArgumentCompleter'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }

    @{ Target = $script:Srv2019_5_profile; Script = '[Microsoft.PowerShell.Commands.WebSslProtocol]::Default -eq "Tls12"'; Types = @('Microsoft.PowerShell.Commands.WebSslProtocol'); Version = "5.1"; OS = 'Windows'; ProblemCount = 1 }

    @{ Target = $script:Ubuntu1804_6_1_profile; Script = '[System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy()'; Types = @('System.Management.Automation.Security.SystemPolicy'); Version = "6.1.2"; OS = 'Linux'; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = '[System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy()'; Types = @('System.Management.Automation.Security.SystemPolicy'); Version = "6.1.2"; OS = 'Linux'; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = '[System.Management.Automation.Security.SystemEnforcementMode]$enforcementMode = "Audit"'; Types = @('System.Management.Automation.Security.SystemEnforcementMode'); Version = "6.1.2"; OS = 'Linux'; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = '$ci = New-Object "Microsoft.PowerShell.Commands.ComputerInfo"'; Types = @('Microsoft.PowerShell.Commands.ComputerInfo'); Version = "6.1.2"; OS = 'Linux'; ProblemCount = 1 }
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