# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.


Describe 'UseConsistentParametersKind' {
    Context 'When preferred parameters kind is set to "ParamBlock" explicitly' {

        BeforeAll {
            $ruleConfiguration = @{
                Enable         = $true
                ParametersKind = "ParamBlock"
            }
            $settings = @{
                IncludeRules = @("PSUseConsistentParametersKind")
                Rules        = @{
                    PSUseConsistentParametersKind = $ruleConfiguration
                }
            }
        }

        It "Returns no violations for parameters outside function" {
            $scriptDefinition = @'
[Parameter()]$FirstParam
[Parameter()]$SecondParam

$FirstParam | Out-Null
$SecondParam | Out-Null
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for param() block outside function" {
            $scriptDefinition = @'
param(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
)

$FirstParam | Out-Null
$SecondParam | Out-Null
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function without parameters" {
            $scriptDefinition = @'
function Test-Function {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with empty param() block" {
            $scriptDefinition = @'
function Test-Function {
    param()
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with non-empty param() block" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter()]$FirstParam,
        [Parameter()]$SecondParam
    )
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with empty inline parameters" {
            $scriptDefinition = @'
function Test-Function() {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with empty inline parameters and non-empty param() block" {
            $scriptDefinition = @'
function Test-Function() {
    param(
        [Parameter()]$FirstParam,
        [Parameter()]$SecondParam
    )
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns violations for function with non-empty inline parameters" {
            $scriptDefinition = @'
function Test-Function(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
) {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 1
        }
    }

    Context 'When preferred parameters kind is set to "ParamBlock" via default value' {

        BeforeAll {
            $ruleConfiguration = @{
                Enable = $true
            }
            $settings = @{
                IncludeRules = @("PSUseConsistentParametersKind")
                Rules        = @{
                    PSUseConsistentParametersKind = $ruleConfiguration
                }
            }
        }

        It "Returns no violations for parameters outside function" {
            $scriptDefinition = @'
[Parameter()]$FirstParam
[Parameter()]$SecondParam

$FirstParam | Out-Null
$SecondParam | Out-Null
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for param() block outside function" {
            $scriptDefinition = @'
param(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
)

$FirstParam | Out-Null
$SecondParam | Out-Null
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function without parameters" {
            $scriptDefinition = @'
function Test-Function {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with empty param() block" {
            $scriptDefinition = @'
function Test-Function {
    param()
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with non-empty param() block" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter()]$FirstParam,
        [Parameter()]$SecondParam
    )
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with empty inline parameters" {
            $scriptDefinition = @'
function Test-Function() {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function with empty inline parameters and non-empty param() block" {
            $scriptDefinition = @'
function Test-Function() {
    param(
        [Parameter()]$FirstParam,
        [Parameter()]$SecondParam
    )
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns violations for function with non-empty inline parameters" {
            $scriptDefinition = @'
function Test-Function(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
) {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 1
        }
    }

    Context 'When preferred parameters kind is set to "Inline" explicitly' {

        BeforeAll {
            $ruleConfiguration = @{
                Enable         = $true
                ParametersKind = "Inline"
            }

            $settings = @{
                IncludeRules = @("PSUseConsistentParametersKind")
                Rules        = @{
                    PSUseConsistentParametersKind = $ruleConfiguration
                }
            }
        }

        It "Returns no violations for parameters outside function" {
            $scriptDefinition = @'
[Parameter()]$FirstParam
[Parameter()]$SecondParam

$FirstParam | Out-Null
$SecondParam | Out-Null
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for param() block outside function" {
            $scriptDefinition = @'
param(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
)

$FirstParam | Out-Null
$SecondParam | Out-Null
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns no violations for function without parameters" {
            $scriptDefinition = @'
function Test-Function {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns violations for function with empty param() block" {
            $scriptDefinition = @'
function Test-Function {
    param()
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Returns violations for function with non-empty param() block" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter()]$FirstParam,
        [Parameter()]$SecondParam
    )
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Returns no violations for function with empty inline parameters" {
            $scriptDefinition = @'
function Test-Function() {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }

        It "Returns violations for function with empty inline parameters and non-empty param() block" {
            $scriptDefinition = @'
function Test-Function() {
    param(
        [Parameter()]$FirstParam,
        [Parameter()]$SecondParam
    )
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Returns no violations for function with non-empty inline parameters" {
            $scriptDefinition = @'
function Test-Function(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
) {
    return
}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }
    }

    Context 'When rule is disabled explicitly' {

        BeforeAll {
            $ruleConfiguration = @{
                Enable         = $false
                ParametersKind = "ParamBlock"
            }
            $settings = @{
                IncludeRules = @("PSUseConsistentParametersKind")
                Rules        = @{
                    PSUseConsistentParametersKind = $ruleConfiguration
                }
            }
        }

        It "Returns no violations for function with non-empty inline parameters" {
            $scriptDefinition = @'
function Test-Function(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
) {
    return
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }
    }

    Context 'When rule is disabled via default "Enable" value' {

        BeforeAll {
            $ruleConfiguration = @{
                ParametersKind = "ParamBlock"
            }
            $settings = @{
                IncludeRules = @("PSUseConsistentParametersKind")
                Rules        = @{
                    PSUseConsistentParametersKind = $ruleConfiguration
                }
            }
        }

        It "Returns no violations for function with non-empty inline parameters" {
            $scriptDefinition = @'
function Test-Function(
    [Parameter()]$FirstParam,
    [Parameter()]$SecondParam
) {
    return
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations | Should -BeNullOrEmpty
        }
    }
}