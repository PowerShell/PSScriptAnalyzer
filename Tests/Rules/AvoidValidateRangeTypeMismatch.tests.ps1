# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'AvoidValidateRangeTypeMismatch' {
    BeforeAll {
        $ruleName = 'PSAvoidValidateRangeTypeMismatch'
    }

    It 'reports integral boundaries for floating-point and decimal parameter types' {
        $scripts = @(
            'function Test { param([double][ValidateRange(1, 2)]$Value) }'
            'function Test { param([single][ValidateRange(-1, 2)]$Value) }'
            'function Test { param([decimal][ValidateRange(1, 2.0)]$Value) }'
            'function Test { param([double][System.Management.Automation.ValidateRangeAttribute(1, 2)]$Value) }'
        )

        foreach ($scriptDefinition in $scripts) {
            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $ruleName
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].Extent.Text | Should -Match 'ValidateRange'
        }
    }

    It 'ignores matching floating-point boundaries and non-floating-point parameters' {
        $scripts = @(
            'function Test { param([double][ValidateRange(1.0, 2.0)]$Value) }'
            'function Test { param([single][ValidateRange(1.0, 2.0)]$Value) }'
            'function Test { param([decimal][ValidateRange(1.0, 2.0)]$Value) }'
            'function Test { param([int][ValidateRange(1, 2)]$Value) }'
        )

        foreach ($scriptDefinition in $scripts) {
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $ruleName | Should -BeNullOrEmpty
        }
    }
}
