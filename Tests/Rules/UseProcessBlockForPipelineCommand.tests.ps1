Describe "UseProcessBlockForPipelineCommand" {
    BeforeAll {
        $RuleName = 'PSUseProcessBlockForPipelineCommand'
        $NoProcessBlock = 'function BadFunc1 { [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) }'
        $NoProcessBlockByPropertyName = 'function $BadFunc2 { [CmdletBinding()] param ([Parameter(ValueFromPipelineByPropertyName)]$Param1) }'
        $HasProcessBlock = 'function GoodFunc1 { [CmdletBinding()] param ([Parameter(ValueFromPipeline)]$Param1) process { } }'
        $HasProcessBlockByPropertyName = 'function GoodFunc2 { [CmdletBinding()] param ([Parameter(ValueFromPipelineByPropertyName)]$Param1) process { } }'
        $NoAttribute = 'function GoodFunc3 { [CmdletBinding()] param ([Parameter()]$Param1) }'
		$HasTypeDeclaration = 'function GoodFunc3 { [CmdletBinding()] param ([Parameter()][string]$Param1) }'
    }

    Context "When there are violations" {
        $Cases = @(
            @{ScriptDefinition = $NoProcessBlock; Name = "NoProcessBlock"}
            @{ScriptDefinition = $NoProcessBlockByPropertyName; Name = "NoProcessBlockByPropertyName"}
        )
        It "has 1 violation for function <Name>" {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should -Not -BeNullOrEmpty
        } -TestCases $Cases
    }

    Context "When there are no violations" {
        $Cases = @(
            @{ScriptDefinition = $HasProcessBlock; Name = "HasProcessBlock" }
            @{ScriptDefinition = $HasProcessBlockByPropertyName; Name = "HasProcessBlockByPropertyName" }
            @{ScriptDefinition = $NoAttribute; Name = "NoAttribute" }
			@{ScriptDefinition = $HasTypeDeclaration; Name = "HasTypeDeclaration"}
        )
        It "has no violations for function <Name>" {
            param ($ScriptDefinition)

            Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName | Should -BeNullOrEmpty
        } -TestCases $Cases
    }
}
