Import-Module PSScriptAnalyzer
$globalMessage = "Found global variable 'Global:1'."
$globalName = "PSAvoidGlobalVars"

# PSAvoidUninitializedVariable rule has been deprecated
# $nonInitializedName = "PSAvoidUninitializedVariable"

$nonInitializedMessage = "Variable 'globalVars' is not initialized. Non-global variables must be initialized. To fix a violation of this rule, please initialize non-global variables."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidGlobalOrUnitializedVars.ps1

# PSAvoidUninitializedVariable rule has been deprecated
# $dscResourceViolations = Invoke-ScriptAnalyzer $directory\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $nonInitializedName}

$globalViolations = $violations | Where-Object {$_.RuleName -eq $globalName}

# PSAvoidUninitializedVariable rule has been deprecated
# $nonInitializedViolations = $violations | Where-Object {$_.RuleName -eq $nonInitializedName}

$noViolations = Invoke-ScriptAnalyzer $directory\AvoidGlobalOrUnitializedVarsNoViolations.ps1
$noGlobalViolations = $noViolations | Where-Object {$_.RuleName -eq $globalName}

# PSAvoidUninitializedVariable rule has been deprecated
# $noUninitializedViolations = $noViolations | Where-Object {$_.RuleName -eq $nonInitializedName}

Describe "AvoidGlobalVars" {
    Context "When there are violations" {
        It "has 1 avoid using global variable violation" {
            $globalViolations.Count | Should Be 1
        }
                
        <#
        # PSAvoidUninitializedVariable rule has been deprecated
        It "has 4 violations for dsc resources (not counting the variables in parameters)" {
            $dscResourceViolations.Count | Should Be 4
        }
        #>


        It "has the correct description message" {
            $globalViolations[0].Message | Should Match $globalMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noGlobalViolations.Count | Should Be 0
        }
    }
}

<#
# PSAvoidUninitializedVariable rule has been deprecated - Hence not a valid test case
Describe "AvoidUnitializedVars" {
    Context "When there are violations" {
        It "has 5 avoid using unitialized variable violations" {
            $nonInitializedViolations.Count | Should Be 5
        }

        It "has the correct description message" {
            $nonInitializedViolations[0].Message | Should Match $nonInitializedMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noUninitializedViolations.Count | Should Be 0
        }
    }    
}
#>