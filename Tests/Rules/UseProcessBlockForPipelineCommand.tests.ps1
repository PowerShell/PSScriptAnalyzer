# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "UseProcessBlockForPipelineCommand" {
    BeforeAll {
        $RuleName = 'PSUseProcessBlockForPipelineCommand'
    }

    Context "When there are violations" {
        $Cases = @(
            @{
                ScriptDefinition = 'function BadFunc1 { [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) }'
                Name = "function without process block"
            }
            @{
                ScriptDefinition = 'function $BadFunc2 { [CmdletBinding()] param ([Parameter(ValueFromPipelineByPropertyName)]$Param1) }'
                Name = "function without process block by property name"
            }
            @{
                ScriptDefinition = '{ [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) }'
                Name = "scriptblock without process block"
            }
        )
        It "has 1 violation for <Name>" -TestCases $Cases {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should -Not -BeNullOrEmpty
        }
    }

    Context "When there are no violations" {
        $Cases = @(
            @{
                ScriptDefinition = 'function GoodFunc1 { [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) process { } }'
                Name = "function with process block"
            }
            @{
                ScriptDefinition = 'function GoodFunc2 { [CmdletBinding()] param ([Parameter(ValueFromPipelineByPropertyName)]$Param1) process { } }'
                Name = "function with process block by property name"
            }
            @{
                ScriptDefinition = 'function GoodFunc3 { [CmdletBinding()] param ([Parameter()]$Param1) }'
                Name = "function without pipeline"
            }
            @{
                ScriptDefinition = 'function GoodFunc3 { [CmdletBinding()] param ([Parameter()][string]$Param1) }'
                Name = "function with parameter type name"
            }
            @{
                ScriptDefinition = '{ [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) process { } }'
                Name = "scriptblock with process block"
            }
        )
        It "has no violations for function <Name>" -TestCases $Cases {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should -BeNullOrEmpty
        }
    }
}
