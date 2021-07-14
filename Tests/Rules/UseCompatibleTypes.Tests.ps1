# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeDiscovery {
    $Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
    $Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
    $Srv2016_5_profile = 'win-8_x64_10.0.14393.0_5.1.14393.2791_x64_4.0.30319.42000_framework'
    $Srv2016_6_2_profile = 'win-8_x64_10.0.14393.0_6.2.4_x64_4.0.30319.42000_core'
    $Srv2016_7_profile = 'win-8_x64_10.0.14393.0_7.0.0_x64_3.1.2_core'
    $Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
    $Srv2019_6_2_profile = 'win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core'
    $Srv2019_7_profile = 'win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core'
    $Win10_5_profile = 'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
    $Win10_6_2_profile = 'win-4_x64_10.0.18362.0_6.2.4_x64_4.0.30319.42000_core'
    $Win10_7_profile = 'win-4_x64_10.0.18362.0_7.0.0_x64_3.1.2_core'
    $Ubuntu1804_6_2_profile = 'ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core'
    $Ubuntu1804_7_profile = 'ubuntu_x64_18.04_7.0.0_x64_3.1.2_core'

    $TypeCompatibilityTestCases = @(
        @{ Target = $Srv2012_3_profile; Script = '[System.Management.Automation.ModuleIntrinsics]::GetModulePath("here", "there", "everywhere")'; Types = @('System.Management.Automation.ModuleIntrinsics'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '$ast -is [System.Management.Automation.Language.FunctionMemberAst]'; Types = @('System.Management.Automation.Language.FunctionMemberAst'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '$version = [System.Management.Automation.SemanticVersion]::Parse($version)'; Types = @('System.Management.Automation.SemanticVersion'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '$kw = New-Object "System.Management.Automation.Language.DynamicKeyword"'; Types = @('System.Management.Automation.Language.DynamicKeyword'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '& { param([Parameter(Position=0)][ArgumentCompleter({"Banana"})][string]$Hello) $Hello } "Banana"'; Types = @('ArgumentCompleter'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2012r2_4_profile; Script = '[WildcardPattern]"bicycle*"'; Types = @('WildcardPattern'); Version = "4.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = '$client = [System.Net.Http.HttpClient]::new()'; Types = @('System.Net.Http.HttpClient'); Version = "4.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = '[Microsoft.PowerShell.EditMode]"Vi"'; Types = @('Microsoft.PowerShell.EditMode'); Version = "4.0"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2019_5_profile; Script = '[Microsoft.PowerShell.Commands.WebSslProtocol]::Default -eq "Tls12"'; Types = @('Microsoft.PowerShell.Commands.WebSslProtocol'); Version = "5.1"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = '[System.Collections.Immutable.ImmutableList[string]]::Empty'; Types = @('System.Collections.Immutable.ImmutableList'); Version = "5.1"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = '[System.Collections.Generic.TreeSet[string]]::new(@("duck", "goose", "banana"))'; Types = @('System.Collections.Generic.TreeSet'); Version = "5.1"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2019_6_2_profile; Script = 'function CertFunc { param([System.Net.ICertificatePolicy]$Policy) Do-Something $Policy }'; Types = @('System.Net.ICertificatePolicy'); Version = "6.2"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2019_7_profile; Script = 'function CertFunc { param([System.Net.ICertificatePolicy]$Policy) Do-Something $Policy }'; Types = @('System.Net.ICertificatePolicy'); Version = "7.0"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Ubuntu1804_6_2_profile; Script = '[System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy()'; Types = @('System.Management.Automation.Security.SystemPolicy'); Version = "6.2"; OS = 'Linux'; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = '[System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy()'; Types = @('System.Management.Automation.Security.SystemPolicy'); Version = "6.2"; OS = 'Linux'; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = '[System.Management.Automation.Security.SystemEnforcementMode]$enforcementMode = "Audit"'; Types = @('System.Management.Automation.Security.SystemEnforcementMode'); Version = "6.2"; OS = 'Linux'; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = '$ci = New-Object "Microsoft.PowerShell.Commands.ComputerInfo"'; Types = @('Microsoft.PowerShell.Commands.ComputerInfo'); Version = "6.2"; OS = 'Linux'; ProblemCount = 1 }

        @{ Target = $Ubuntu1804_7_profile; Script = '[System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy()'; Types = @('System.Management.Automation.Security.SystemPolicy'); Version = "7.0"; OS = 'Linux'; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = '[System.Management.Automation.Security.SystemPolicy]::GetSystemLockdownPolicy()'; Types = @('System.Management.Automation.Security.SystemPolicy'); Version = "7.0"; OS = 'Linux'; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = '[System.Management.Automation.Security.SystemEnforcementMode]$enforcementMode = "Audit"'; Types = @('System.Management.Automation.Security.SystemEnforcementMode'); Version = "7.0"; OS = 'Linux'; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = '$ci = New-Object "Microsoft.PowerShell.Commands.ComputerInfo"'; Types = @('Microsoft.PowerShell.Commands.ComputerInfo'); Version = "7.0"; OS = 'Linux'; ProblemCount = 1 }
    )

    $MemberCompatibilityTestCases = @(
        @{ Target = $Srv2012_3_profile; Script = '[System.Management.Automation.LanguagePrimitives]::ConvertTypeNameToPSTypeName("System.String")'; Types = @('System.Management.Automation.LanguagePrimitives'); Members = @('ConvertTypeNameToPSTypeName'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '[System.Management.Automation.WildcardPattern]::Get("banana*", "None").IsMatch("bananaduck")'; Types = @('System.Management.Automation.WildcardPattern'); Members = @('Get'); Version = "3.0"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2012r2_4_profile; Script = 'if (-not [Microsoft.PowerShell.Commands.ModuleSpecification]::TryParse($msStr, [ref]$modSpec)){ throw "Bad!" }'; Types = @('Microsoft.PowerShell.Commands.ModuleSpecification'); Members = @('TryParse'); Version = "4.0"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = '[System.Management.Automation.LanguagePrimitives]::IsObjectEnumerable($obj)'; Types = @('System.Management.Automation.LanguagePrimitives'); Members = @('IsObjectEnumerable'); Version = "4.0"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2019_5_profile; Script = '$socket = [System.Net.WebSockets.WebSocket]::CreateFromStream($stream, $true, "http", [timespan]::FromMinutes(10))'; Types = @('System.Net.WebSockets.WebSocket'); Members = @('CreateFromStream'); Version = "5.1"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = '[System.Management.Automation.HostUtilities]::InvokeOnRunspace($command, $runspace)'; Types = @('System.Management.Automation.HostUtilities'); Members = @('InvokeOnRunspace'); Version = "5.1"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Ubuntu1804_6_2_profile; Script = '[System.Management.Automation.Tracing.Tracer]::GetExceptionString($e)'; Types = @('System.Management.Automation.Tracing.Tracer'); Members = @('GetExceptionString'); Version = "6.2"; OS = 'Linux'; ProblemCount = 1 }

        @{ Target = $Ubuntu1804_7_profile; Script = '[System.Management.Automation.Tracing.Tracer]::GetExceptionString($e)'; Types = @('System.Management.Automation.Tracing.Tracer'); Members = @('GetExceptionString'); Version = "7.0"; OS = 'Linux'; ProblemCount = 1 }
    )
}

Describe 'UseCompatibleTypes' {
    BeforeAll {
        $RuleName = 'PSUseCompatibleTypes'

        $TargetProfileConfigKey = 'TargetProfiles'

        $Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
        $Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
        $Srv2016_5_profile = 'win-8_x64_10.0.14393.0_5.1.14393.2791_x64_4.0.30319.42000_framework'
        $Srv2016_6_2_profile = 'win-8_x64_10.0.14393.0_6.2.4_x64_4.0.30319.42000_core'
        $Srv2016_7_profile = 'win-8_x64_10.0.14393.0_7.0.0_x64_3.1.2_core'
        $Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
        $Srv2019_6_2_profile = 'win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core'
        $Srv2019_7_profile = 'win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core'
        $Win10_5_profile = 'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
        $Win10_6_2_profile = 'win-4_x64_10.0.18362.0_6.2.4_x64_4.0.30319.42000_core'
        $Win10_7_profile = 'win-4_x64_10.0.18362.0_7.0.0_x64_3.1.2_core'
        $Ubuntu1804_6_2_profile = 'ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core'
        $Ubuntu1804_7_profile = 'ubuntu_x64_18.04_7.0.0_x64_3.1.2_core'

        $AzF_profile = (Resolve-Path "$PSScriptRoot/../../PSCompatibilityCollector/optional_profiles/azurefunctions.json").Path
        $AzA_profile = (Resolve-Path "$PSScriptRoot/../../PSCompatibilityCollector/optional_profiles/azureautomation.json").Path
    }

    Context 'Targeting a single profile' {
        It "Reports <ProblemCount> problem(s) with <Script> on <OS> with PowerShell <Version> targeting <Target>" -TestCases $TypeCompatibilityTestCases {
            param($Script, [string]$Target, [string[]]$Types, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $RuleName -ScriptDefinition $Script -Settings $settings

            $diagnostics.Count | Should -Be $ProblemCount

            for ($i = 0; $i -lt $diagnostics.Count; $i++)
            {
                $diagnostics[$i].Type | Should -BeExactly $Types[$i]
                $diagnostics[$i].TargetPlatform.OperatingSystem.Family | Should -Be $OS
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Major | Should -Be $Version.Major
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Minor | Should -Be $Version.Minor
            }
        }

        It "Reports <ProblemCount> problem(s) with <Script> on <OS> with PowerShell <Version> targeting <Target>" -TestCases $MemberCompatibilityTestCases {
            param($Script, [string]$Target, [string[]]$Types, [string[]]$Members, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $RuleName -ScriptDefinition $Script -Settings $settings

            $diagnostics.Count | Should -Be $ProblemCount

            for ($i = 0; $i -lt $diagnostics.Count; $i++)
            {
                $diagnostics[$i].Type | Should -BeExactly $Types[$i]
                $diagnostics[$i].Member | Should -BeExactly $Members[$i]
                $diagnostics[$i].TargetPlatform.OperatingSystem.Family | Should -Be $OS
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Major | Should -Be $Version.Major
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Minor | Should -Be $Version.Minor
            }
        }
    }

    Context "Full file checking against all targets" {
        It "Finds all incompatibilities in the script" {
            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @(
                            $Srv2012_3_profile
                            $Srv2012r2_4_profile
                            $Srv2016_5_profile
                            $Srv2016_6_2_profile
                            $Srv2019_5_profile
                            $Srv2019_6_2_profile
                            $Win10_5_profile
                            $Win10_6_2_profile
                            $Ubuntu1804_6_2_profile
                        )
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -Path "$PSScriptRoot/CompatibilityRuleAssets/IncompatibleScript.ps1" -Settings $settings -IncludeRule PSUseCompatibleTypes -Severity Information,Warning,Error

            # Classes in the script cause extra diagnostics in PS 3 and 4
            $diagnostics.Count | Should -Be 2
            foreach ($diagnostic in $diagnostics)
            {
                $diagnostic.Member | Should -BeExactly 'TryParse'
                $diagnostic.Type | Should -BeExactly 'Microsoft.PowerShell.Commands.ModuleSpecification'
            }
        }
    }

    Context "PSSA repository code checking" {
        It "Checks that there are no incompatibilities in PSSA build scripts" {
            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @(
                            $Srv2012_3_profile
                            $Srv2012r2_4_profile
                            $Srv2016_5_profile
                            $Srv2016_6_2_profile
                            $Srv2019_7_profile
                            $Srv2019_5_profile
                            $Srv2019_6_2_profile
                            $Win10_5_profile
                            $Win10_6_2_profile
                            $Ubuntu1804_6_2_profile
                            $Ubuntu1804_7_profile
                        )
                        IgnoreTypes = @('System.IO.Compression.ZipFile')
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -Path "$PSScriptRoot/../../" -Settings $settings -IncludeRule PSUseCompatibleTypes
            $diagnostics.Count | Should -Be 0
        }
    }

    Context 'Targeting new-form Az profiles alongside older profiles' {
        BeforeAll {
            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @(
                            $AzF_profile
                            $AzA_profile
                            $Win10_5_profile
                        )
                    }
                }
            }
        }

        It "Finds AzF problems with a script" {
            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $RuleName -Settings $settings -ScriptDefinition '
            [System.Collections.Immutable.ImmutableList[string]]::Empty
            [Microsoft.PowerShell.ToStringCodeMethods]::PropertyValueCollection($obj)
            '

            $diagnostics.Count | Should -Be 2
            $diagnosticGroups = Group-Object -InputObject $diagnostics -Property Type
            foreach ($group in $diagnosticGroups)
            {
                switch ($group.Name)
                {
                    'System.Collections.Immutable.ImmutableList[System.String]'
                    {
                        $group.Count | Should -Be 2
                        $group[0].Group.TargetPlatform.PowerShell.Version.Major | Should -Be 5
                        $group[1].Group.TargetPlatform.PowerShell.Version.Major | Should -Be 5
                        break
                    }

                    'Microsoft.PowerShellToStringCodeMethods'
                    {
                        $group.Count | Should -Be 1
                        $group[0].Group.TargetPlatform.PowerShell.Version.Major | Should -Be 6
                        break
                    }
                }
            }
        }
    }
}