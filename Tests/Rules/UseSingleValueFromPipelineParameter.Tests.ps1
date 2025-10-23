# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = 'PSUseSingleValueFromPipelineParameter'

    $settings = @{
        IncludeRules = @($ruleName)
        Rules        = @{
            $ruleName = @{
                Enable = $true
            }
        }
    }
}

Describe 'UseSingleValueFromPipelineParameter' {

    Context 'When multiple parameters have ValueFromPipeline in same parameter set' {

        It "Should flag explicit ValueFromPipeline=`$true in default parameter set" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        $InputObject,

        [Parameter(ValueFromPipeline=$true)]
        $AnotherParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 2
            $violations[0].Message | Should -Match "Multiple parameters \(InputObject, AnotherParam\) in parameter set 'default'"
        }

        It 'Should flag implicit ValueFromPipeline in default parameter set' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline)]
        $InputObject,

        [Parameter(ValueFromPipeline)]
        $SecondParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 2
        }

        It 'Should flag mixed explicit and implicit ValueFromPipeline' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        $InputObject,

        [Parameter(ValueFromPipeline)]
        $SecondParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 2
        }

        It 'Should flag multiple parameters in named parameter set' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true, ParameterSetName='MySet')]
        $InputObject,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='MySet')]
        $SecondParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 2
            $violations[0].Message | Should -Match "parameter set 'MySet'"
        }

        It 'Should flag three parameters in same parameter set' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        $First,

        [Parameter(ValueFromPipeline=$true)]
        $Second,

        [Parameter(ValueFromPipeline=$true)]
        $Third
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 3
            $violations[0].Message | Should -Match 'Multiple parameters \(First, Second, Third\)'
        }
    }

    Context 'When parameters are in different parameter sets' {

        It 'Should not flag parameters in different parameter sets' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true, ParameterSetName='Set1')]
        $InputObject1,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='Set2')]
        $InputObject2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }

        It 'Should handle mix of named and default parameter sets correctly' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        $DefaultSetParam,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='NamedSet')]
        $NamedSetParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context 'When only one parameter has ValueFromPipeline' {

        It 'Should not flag single ValueFromPipeline parameter' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        $InputObject,

        [Parameter()]
        $OtherParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context 'When ValueFromPipeline is explicitly set to false' {

        It "Should not flag parameters with ValueFromPipeline=`$false" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$false)]
        $InputObject,

        [Parameter(ValueFromPipeline=$false)]
        $AnotherParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }

        It 'Should only flag the true ValueFromPipeline parameter' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        $TrueParam,

        [Parameter(ValueFromPipeline=$false)]
        $FalseParam,

        [Parameter()]
        $NoValueParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context 'When non-Parameter attributes have ValueFromPipeline property' {

        It 'Should not flag custom attributes with ValueFromPipeline property' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        [CustomAttribute(ValueFromPipeline=$true)]
        $InputObject,

        [CustomAttribute(ValueFromPipeline=$true)]
        $NonPipelineParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }

        It 'Should not flag ValidateSet with ValueFromPipeline property' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true)]
        [ValidateSet('Value1', 'Value2', ValueFromPipeline=$true)]
        $InputObject,

        [ValidateSet('Value1', 'Value2', ValueFromPipeline=$true)]
        $NonPipelineParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context 'When there are no Parameter attributes' {

        It 'Should not flag functions without Parameter attributes' {
            $scriptDefinition = @'
function Test-Function {
    param(
        $InputObject,
        $AnotherParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }

        It 'Should not flag functions with only non-ValueFromPipeline Parameter attributes' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(Mandatory=$true)]
        $InputObject,

        [Parameter(Position=0)]
        $AnotherParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context 'Complex parameter set scenarios' {

        It 'Should flag violations in multiple parameter sets independently' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true, ParameterSetName='Set1')]
        $Set1Param1,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='Set1')]
        $Set1Param2,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='Set2')]
        $Set2Param1,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='Set2')]
        $Set2Param2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 4  # 2 violations per parameter set, each parameter gets flagged

            # Check that both parameter sets are mentioned in violations
            $violationMessages = $violations.Message -join ' '
            $violationMessages | Should -Match "parameter set 'Set1'"
            $violationMessages | Should -Match "parameter set 'Set2'"
        }

        It 'Should handle __AllParameterSets parameter set name correctly' {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ValueFromPipeline=$true, ParameterSetName='__AllParameterSets')]
        $ExplicitAllSets,

        [Parameter(ValueFromPipeline=$true)]
        $ImplicitAllSets
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 2
            $violations[0].Message | Should -Match "parameter set 'default'"
        }
    }

    Context 'Suppression scenarios' {

        It 'Should be suppressible by parameter set name' {
            $scriptDefinition = @'
function Test-Function {
    [Diagnostics.CodeAnalysis.SuppressMessage('PSUseSingleValueFromPipelineParameter', 'MySet')]
    param(
        [Parameter(ValueFromPipeline=$true, ParameterSetName='MySet')]
        $InputObject,

        [Parameter(ValueFromPipeline=$true, ParameterSetName='MySet')]
        $AnotherParam
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }
}