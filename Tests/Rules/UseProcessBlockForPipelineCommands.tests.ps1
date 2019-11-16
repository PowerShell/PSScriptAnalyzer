Describe "UseProcessBlockForPipelineCommands" {
    BeforeAll {
        $RuleName = 'PSUseProcessBlockForPipelineCommands'
        $WithoutProcessBlock = 'function BadFunc1 { [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) }'
        $WithoutProcessBlockByPropertyName = 'function $BadFunc2 { [CmdletBinding()] param ([Parameter(ValueFromPipelineByPropertyName)]$Param1) }'
        $WithProcessBlock = 'function GoodFunc1 { [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) process { } }'
        $WithProcessBlockByPropertyName = 'function GoodFunc2 { [CmdletBinding()] param ([Parameter(ValueFromPipelineByPropertyName)]$Param1) process { } }'
    }

    Context "When there are violations" {
        $Cases = @(
            @{ScriptDefinition = $WithoutProcessBlock}
            @{ScriptDefinition = $WithoutProcessBlockByPropertyName}
        )
        It "has 1 violation for function <ScriptDefinition>" {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should Not BeNullOrEmpty
        } -TestCases $Cases
    }

    Context "When there are no violations" {
        $Cases = @(
            @{ScriptDefinition = $WithProcessBlock }
            @{ScriptDefinition = $WithProcessBlockByPropertyName }
        )
        It "has no violations for function <ScriptDefinition>" {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should BeNullOrEmpty
        } -TestCases $Cases
    }
}
