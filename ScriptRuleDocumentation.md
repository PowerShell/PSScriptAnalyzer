## Documentation for Customized Rules in PowerShell Scripts

PSScriptAnalyzer uses MEF(Managed Extensibility Framework) to import all rules defined in the assembly. It can also consume rules written in PowerShell scripts.

When calling Invoke-ScriptAnalyzer, users can specify custom rules using the parameter `CustomizedRulePath`.

The purpose of this documentation is to server as a basic guide on creating your own customized rules.

### Basics

- Functions should have comment-based help. Make sure .DESCRIPTION field is there, as it will be consumed as rule description for the customized rule.

``` PowerShell
<#
.SYNOPSIS
    Name of your rule.
.DESCRIPTION
    This would be the description of your rule. Please refer to Rule Documentation for consistent rule messages.
.EXAMPLE
.INPUTS
.OUTPUTS
.NOTES
#>
```

- Output type should be DiagnosticRecord:

``` PowerShell
[OutputType([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
```

- Make sure each function takes either a Token or an Ast as a parameter

``` PowerShell
Param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [System.Management.Automation.Language.ScriptBlockAst]
    $testAst
)
```

- DiagnosticRecord should have four properties: Message, Extent, RuleName and Severity

``` PowerShell
$result = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]]@{
    "Message"  = "This is a sample rule"
    "Extent"   = $ast.Extent
    "RuleName" = $PSCmdlet.MyInvocation.InvocationName
    "Severity" = "Warning"
}
```

- Make sure you export the function(s) at the end of the script using Export-ModuleMember

``` PowerShell
Export-ModuleMember -Function (FunctionName)
```

### Example

``` PowerShell
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
        [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]]
        .NOTES
        None
#>
function Measure-RequiresRunAsAdministrator
{
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        $results = @()
        try
        {
            #region Define predicates to find ASTs.
            # Finds specific method, IsInRole.
            [ScriptBlock]$predicate1 = {
                param ([System.Management.Automation.Language.Ast]$Ast)
                [bool]$returnValue = $false
                if ($Ast -is [System.Management.Automation.Language.MemberExpressionAst])
                {
                    [System.Management.Automation.Language.MemberExpressionAst]$meAst = $Ast
                    if ($meAst.Member -is [System.Management.Automation.Language.StringConstantExpressionAst])
                    {
                        [System.Management.Automation.Language.StringConstantExpressionAst]$sceAst = $meAst.Member
                        if ($sceAst.Value -eq 'isinrole')
                        {
                            $returnValue = $true
                        }
                    }
                }
                return $returnValue
            }

            # Finds specific value, [system.security.principal.windowsbuiltinrole]::administrator.
            [ScriptBlock]$predicate2 = {
                param ([System.Management.Automation.Language.Ast]$Ast)
                [bool]$returnValue = $false
                if ($Ast -is [System.Management.Automation.Language.AssignmentStatementAst])
                {
                    [System.Management.Automation.Language.AssignmentStatementAst]$asAst = $Ast
                    if ($asAst.Right.ToString().ToLower() -eq '[system.security.principal.windowsbuiltinrole]::administrator')
                    {
                        $returnValue = $true
                    }
                }
                return $returnValue
            }
            #endregion
            #region Finds ASTs that match the predicates.

            [System.Management.Automation.Language.Ast[]]$methodAst     = $ScriptBlockAst.FindAll($predicate1, $true)
            [System.Management.Automation.Language.Ast[]]$assignmentAst = $ScriptBlockAst.FindAll($predicate2, $true)
            if ($null -ne $ScriptBlockAst.ScriptRequirements)
            {
                if ((!$ScriptBlockAst.ScriptRequirements.IsElevationRequired) -and
                ($methodAst.Count -ne 0) -and ($assignmentAst.Count -ne 0))
                {
                    $result = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord]@{
                        'Message' = $Messages.MeasureRequiresRunAsAdministrator
                        'Extent' = $assignmentAst.Extent
                        'RuleName' = $PSCmdlet.MyInvocation.InvocationName
                        'Severity' = 'Information'
                    }
                    $results += $result
                }
            }
            else
            {
                if (($methodAst.Count -ne 0) -and ($assignmentAst.Count -ne 0))
                {
                    $result = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord]@{
                        'Message' = $Messages.MeasureRequiresRunAsAdministrator
                        'Extent' = $assignmentAst.Extent
                        'RuleName' = $PSCmdlet.MyInvocation.InvocationName
                        'Severity' = 'Information'
                    }
                    $results += $result
                }
            }
            return $results
            #endregion
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}
```

More examples can be found in *Tests\Engine\CommunityRules*
