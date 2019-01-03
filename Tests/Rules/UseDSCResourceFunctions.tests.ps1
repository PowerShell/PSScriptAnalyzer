
$violationMessage = "Missing 'Get-TargetResource' function. DSC Resource must implement Get, Set and Test-TargetResource functions."
$classViolationMessage = "Missing 'Set' function. DSC Class must implement Get, Set and Test functions."
$violationName = "PSDSCStandardDSCFunctionsInResource"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\DSCResourceModule\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}

if ($PSVersionTable.PSVersion -ge [Version]'5.0.0')
{
    $classViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResourceModule\DSCResources\BadDscResource\BadDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "StandardDSCFunctionsInResource" {
    Context "When there are violations" {
        It "has 1 missing standard DSC functions violation" {
            Write-Host $violations[0].RuleName
            Write-Host $violations[1].RuleName
            Write-Host $violations[2].RuleName
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            Write-Host $noViolations[0].RuleName
            Write-Host $noViolations[1].RuleName
            Write-Host $noViolations[2].RuleName
            $noViolations.Count | Should -Be 0
        }
    }
}

if ($PSVersionTable.PSVersion -ge [Version]'5.0.0') {
 Describe "StandardDSCFunctionsInClass" {
    Context "When there are violations" {
        It "has 1 missing standard DSC functions violation" {
            $classViolations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $classViolations[0].Message | Should -Match $classViolationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noClassViolations.Count | Should -Be 0
        }
    }
 }
}
