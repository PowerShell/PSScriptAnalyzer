# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSUseLiteralInitializerForHashtable"
}

Describe "UseLiteralInitlializerForHashtable" {
    Context "When new-object hashtable is used to create a hashtable" {
        It "has violation" {
            $violationScriptDef = @'
            $htable1 = new-object hashtable
            $htable2 = new-object system.collections.hashtable
            $htable3 = new-object -Typename hashtable -ArgumentList 10
            $htable4 = new-object collections.hashtable
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $violationScriptDef -IncludeRule $ruleName
            $violations.Count | Should -Be 4
        }

        It "does not detect violation if arguments given to new-object contain ignore case string comparer" {
            $violationScriptDef = @'
            $htable1 = new-object hashtable [system.stringcomparer]::ordinalignorecase
            $htable2 = new-object -Typename hashtable -ArgumentList [system.stringcomparer]::ordinalignorecase
            $htable3 = new-object hashtable -ArgumentList [system.stringcomparer]::ordinalignorecase
            $htable4 = new-object -Typename hashtable [system.stringcomparer]::ordinalignorecase
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $violationScriptDef -IncludeRule $ruleName
            $violations.Count | Should -Be 0
        }

        It "suggests correction" {
            $violationScriptDef = @'
            $htable1 = new-object hashtable
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $violationScriptDef -IncludeRule $ruleName
            $violations[0].SuggestedCorrections[0].Text | Should -Be '@{}'
        }
    }

    Context "When [hashtable]::new is used to create a hashtable" {
        It "has violation" {
            $violationScriptDef = @'
            $htable1 = [hashtable]::new()
            $htable2 = [system.collections.hashtable]::new()
            $htable3 = [hashtable]::new(10)
            $htable4 = [collections.hashtable]::new()
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $violationScriptDef -IncludeRule $ruleName
            $violations.Count | Should -Be 4
        }

        It "does not detect violation if arguments given to [hashtable]::new contain ignore case string comparer" {
            $violationScriptDef = @'
            $htable1 = [hashtable]::new(10, [system.stringcomparer]::ordinalignorecase)
            $htable2 = [hashtable]::new(10, [system.stringcomparer]::ordinalignorecase)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $violationScriptDef -IncludeRule $ruleName
            $violations.Count | Should -Be 0
        }

        It "suggests correction" {
            $violationScriptDef = @'
            $htable1 = [hashtable]::new()
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $violationScriptDef -IncludeRule $ruleName
            $violations[0].SuggestedCorrections[0].Text | Should -Be '@{}'
        }

    }
}
