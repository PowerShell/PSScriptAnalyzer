Import-Module PSScriptAnalyzer
$WMIRuleName = "PSAvoidUsingWMICmdlet"
$violationMessage = "File 'AvoidUsingWMICmdlet.ps1' uses WMI cmdlet. For PowerShell 3.0 and above, use CIM cmdlet which perform the same tasks as the WMI cmdlets. The CIM cmdlets comply with WS-Management (WSMan) standards and with the Common Information Model (CIM) standard, which enables the cmdlets to use the same techniques to manage Windows computers and those running other operating systems."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingWMICmdlet.ps1 -IncludeRule $WMIRuleName
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingWMICmdletNoViolations.ps1 -IncludeRule $WMIRuleName

Describe "AvoidUsingWMICmdlet" {
    Context "Script contains references to WMI cmdlets - Violation" {
        It "Have 5 WMI cmdlet Violations" {
            $violations.Count | Should Be 5
        }

        It "has the correct description message for WMI rule violation" {
            $violations[0].Message | Should Be $violationMessage            
        }
    }

    Context "Script contains no calls to WMI cmdlet - No violation" {
        It "results in no rule violations" {
            $noViolations.Count | Should Be 0
        }
    }
}