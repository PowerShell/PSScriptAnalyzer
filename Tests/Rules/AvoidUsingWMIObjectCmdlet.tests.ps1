Import-Module PSScriptAnalyzer
$wmiObjectRuleName = "PSAvoidUsingWMIObjectCmdlet"
$violationMessage = "File 'AvoidUsingWMIObjectCmdlet.ps1' uses WMIObject cmdlet. For PowerShell 3.0 and above, this is not recommended because the cmdlet is based on a non-standard DCOM protocol. Use CIMInstance cmdlet instead. This is CIM and WS-Man standards compliant and works in a heterogeneous environment."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingWMIObjectCmdlet.ps1 | Where-Object {$_.RuleName -eq $wmiObjectRuleName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingWMIObjectCmdletNoViolations.ps1 | Where-Object {$_.RuleName -eq $wmiObjectRuleName}

Describe "AvoidUsingWMIObjectCmdlet" {
    Context "Script contains references to WMIObject cmdlets - Violation" {
        It "Have 2 WMIObject cmdlet Violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message for WMIObject rule violation" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "Script contains no calls to WMIObject cmdlet - No violation" {
        It "results in no rule violations" {
            $noViolations.Count | Should Be 0
        }
    }
}