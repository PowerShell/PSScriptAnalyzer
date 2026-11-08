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
            @{
                ScriptDefinition = 'param ([Parameter(ValueFromPipeline=$true)]$Item) end { $global:input }'
                Name = "end block that reads a different scope's input"
            }
            @{
                ScriptDefinition = 'param ([Parameter(ValueFromPipeline=$true)]$Item) end { { $input } }'
                Name = "input only referenced in a nested script block"
            }
            @{
                ScriptDefinition = 'param ([Parameter(ValueFromPipeline=$true)]$Item) end { $input = 1 }'
                Name = "end block that only assigns to input"
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
            @{
                ScriptDefinition = 'param ([Parameter(ValueFromPipeline=$false, ValueFromPipelineByPropertyName=$false)]$Item)'
                Name = "explicitly disabled pipeline binding"
            }
            @{
                ScriptDefinition = 'param ([Parameter(ValueFromPipeline=$true)]$Item) end { $input | ConvertTo-Json }'
                Name = "explicit end block that consumes pipeline input"
            }
            @{
                ScriptDefinition = 'param ([Parameter(ValueFromPipeline)]$Item) $local:input | ConvertTo-Json'
                Name = "implicit end block that consumes pipeline input"
            }
        )
        It "has no violations for function <Name>" -TestCases $Cases {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should -BeNullOrEmpty
        }
    }
}
