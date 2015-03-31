Import-Module -Verbose ScriptAnalyzer

$violationMessage = "Missing Get-TargetResource function. DSC Resource must implement Get, Set and Test-TargetResource functions."
$violationName = "PSDSCStandardDSCFunctionsInResource"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}
$classViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResources\BadDscResource\BadDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}

Describe "StandardDSCFunctionsInResource" {
    Context "When there are violations" {
        It "has 1 missing standard DSC functions violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}

Describe "StandardDSCFunctionsInClass" {
    Context "When there are violations" {
        It "has 1 missing standard DSC functions violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}