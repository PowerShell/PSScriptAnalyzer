# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $globalMessage = "Found global variable 'Global:1'."
    $globalName = "PSAvoidGlobalVars"

    $nonInitializedMessage = "Variable 'globalVars' is not initialized. Non-global variables must be initialized. To fix a violation of this rule, please initialize non-global variables."
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidGlobalOrUnitializedVars.ps1

    $globalViolations = $violations | Where-Object {$_.RuleName -eq $globalName}

    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidGlobalOrUnitializedVarsNoViolations.ps1
    $noGlobalViolations = $noViolations | Where-Object {$_.RuleName -eq $globalName}
}

Describe "AvoidGlobalVars" {
    Context "When there are violations" {
        It "has 1 avoid using global variable violation" {
            $globalViolations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $globalViolations[0].Message | Should -Match $globalMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noGlobalViolations.Count | Should -Be 0
        }
    }

    Context "When a script contains global:lastexitcode" {
        It "returns no violation" {
            $def = @'
if ($global:lastexitcode -ne 0)
{
    exit
}
'@
            $local:violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -IncludeRule $globalName
            $local:violations.Count | Should -Be 0
        }
    }
}
