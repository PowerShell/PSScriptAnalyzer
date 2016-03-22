Import-Module PSScriptAnalyzer

$violationMessage = "There is no call to Write-Verbose in DSC function ‘Test-TargetResource’. If you are using Write-Verbose in a helper function, suppress this rule application."
$violationName = "PSDSCUseVerboseMessageInDSCResource"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}

Describe "UseVerboseMessageInDSCResource" {
    Context "When there are violations" {
        It "has 2 Verbose Message violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }        
    }
}