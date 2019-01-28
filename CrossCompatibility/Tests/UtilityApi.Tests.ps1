Import-Module "$PSScriptRoot/../out/CrossCompatibility" -Force -ErrorAction Stop

Describe "Type name transformation" {
    BeforeAll {
        $typeNameTestCases = @(
            @{ InputType = [System.Reflection.Assembly]; ExpectedName = "System.Reflection.Assembly" }
            @{ InputType = [string]; ExpectedName = "System.String" }
            @{ InputType = [datetime]; ExpectedName = "System.DateTime" }
            @{ InputType = [string[]]; ExpectedName = "System.String[]" }
            @{ InputType = [System.Collections.Generic.List[object]]; ExpectedName = "System.Collections.Generic.List``1[System.Object]" }
            @{ InputType = [System.Collections.Generic.Dictionary[string, object]]; ExpectedName = "System.Collections.Generic.Dictionary``2[System.String,System.Object]" }
            @{ InputType = [System.Func`1]; ExpectedName = "System.Func``1[TResult]" }
            @{ InputType = [System.Collections.Generic.Dictionary`2]; ExpectedName = "System.Collections.Generic.Dictionary``2[TKey,TValue]" }
            @{ InputType = [System.Collections.Generic.Dictionary`2+Enumerator]; ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator[TKey,TValue]" }
            @{ InputType = [System.Collections.Generic.Dictionary[string,object]].GetNestedType('Enumerator'); ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator[TKey,TValue]" }
            @{ InputType = [System.Collections.Concurrent.ConcurrentDictionary`2].GetMethod('ToArray').ReturnType; ExpectedName = "System.Collections.Generic.KeyValuePair``2[TKey,TValue][]"}
        )
    }

    It "Serializes the name of type <InputType> to <ExpectedName>" -TestCases $typeNameTestCases {
        param([type]$InputType, [string]$ExpectedName)

        $name = [Microsoft.PowerShell.CrossCompatibility.Utility.TypeDataConversion]::GetFullTypeName($InputType)
        $name | Should -BeExactly $ExpectedName
    }

    It "Null type gives null type name" {
        [Microsoft.PowerShell.CrossCompatibility.Utility.TypeDataConversion]::GetFullTypeName($null) | Should -Be $null
    }
}