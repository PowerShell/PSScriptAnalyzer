#Requires -Version 3.0

<#
.SYNOPSIS
    Uses #Requires -RunAsAdministrator instead of your own methods.
.DESCRIPTION
    The #Requires statement prevents a script from running unless the Windows PowerShell version, modules, snap-ins, and module and snap-in version prerequisites are met.
    From Windows PowerShell 4.0, the #Requires statement let script developers require that sessions be run with elevated user rights (run as Administrator).
    Script developers does not need to write their own methods any more.
    To fix a violation of this rule, please consider to use #Requires -RunAsAdministrator instead of your own methods.
.EXAMPLE
    Measure-RequiresRunAsAdministrator -ScriptBlockAst $ScriptBlockAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [OutputType([PSCustomObject[])]
.NOTES
    None
#>
function Measure-RequiresRunAsAdministrator
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $testAst
    )

    # The implementation is mocked out for testing purposes only and many properties are deliberately set to null to test if PSSA can cope with it
    $l=(new-object System.Collections.ObjectModel.Collection["Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent"])
    $c = (new-object Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent 1,2,3,4,'text','filePath','description')
    $l.Add($c)
    $extent = $null
    $dr = New-Object `
            -Typename "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord" `
            -ArgumentList "This is help",$extent,$PSCmdlet.MyInvocation.InvocationName,Warning,$null,$null,$l
    $dr.RuleSuppressionID = "MyRuleSuppressionID"
    return $dr
}
Export-ModuleMember -Function Measure*