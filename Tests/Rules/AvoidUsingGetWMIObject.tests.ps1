Import-Module PSScriptAnalyzer
$getWMIObjectRuleName = "PSAvoidUsingGetWMIObject"
$violationMessage = "File 'AvoidUsingGetWMIObject.ps1' uses Get-WMIObject. For PowerShell 3.0 and above, this is not recommended because the cmdlet is based on a non-standard DCOM protocol. Use Get-CIMInstance instead. This is CIM and WS-Man standards compliant and works in a heterogeneous environment."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingGetWMIObject.ps1 | Where-Object {$_.RuleName -eq $getWMIObjectRuleName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingGetWMIObjectNoViolations.ps1 | Where-Object {$_.RuleName -eq $getWMIObjectRuleName}

Describe "AvoidUsingGetWMIObject" {
    Context "Script contains references to Get-WMIObject - Violation" {
        It "Have 2 Get-WMIObject Violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message for Get-WMIObject" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "Script contains no calls to Get-WMIObject - No violation" {
        It "results in no rule violations" {
            $noViolations.Count | Should Be 0
        }
    }
}