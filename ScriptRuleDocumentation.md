## Documentation for Customized Rules in PowerShell Scripts

PSScriptAnalyzer uses MEF(Managed Extensibility Framework) to import all rules defined in the assembly. It can also consume rules written in PowerShell scripts.

When calling Invoke-ScriptAnalyzer, users can specify custom rules using the parameter `CustomizedRulePath`.

The purpose of this documentation is to serve as a basic guide on creating your own customized rules.

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

- Make sure each function takes either a Token array or an Ast as a parameter. The _Ast_ parameter name must end with 'Ast' and the _Token_ parameter name must end with 'Token'

``` PowerShell
Param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [System.Management.Automation.Language.ScriptBlockAst]
    $testAst
)
```

``` PowerShell
Param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [System.Management.Automation.Language.Token[]]
    $testToken
)
```

- DiagnosticRecord should have at least four properties: Message, Extent, RuleName and Severity

``` PowerShell
$result = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]]@{
    "Message"  = "This is a sample rule"
    "Extent"   = $ast.Extent
    "RuleName" = $PSCmdlet.MyInvocation.InvocationName
    "Severity" = "Warning"
}
```
Optionally, since version 1.17.0, a `SuggestedCorrections` property of type `IEnumerable<CorrectionExtent>` can also be added in script rules but care must be taken that the type is correct, an example is:
```powershell
[int]$startLineNumber =  $ast.Extent.StartLineNumber
[int]$endLineNumber = $ast.Extent.EndLineNumber
[int]$startColumnNumber = $ast.Extent.StartColumnNumber
[int]$endColumnNumber = $ast.Extent.EndColumnNumber
[string]$correction = 'Correct text that replaces Extent text'
[string]$file = $MyInvocation.MyCommand.Definition
[string]$optionalDescription = 'Useful but optional description text'
$correctionExtent = New-Object 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent' $startLineNumber,$endLineNumber,$startColumnNumber,$endColumnNumber,$correction,$description
$suggestedCorrections = New-Object System.Collections.ObjectModel.Collection['Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent']
$suggestedCorrections.add($correctionExtent) | out-null

[Microsoft.Windows.Powershell.ScriptAnalyzer.Generic.DiagnosticRecord]@{
    "Message"              = "This is a rule with a suggested correction"
    "Extent"               = $ast.Extent
    "RuleName"             = $PSCmdlet.MyInvocation.InvocationName
    "Severity"             = "Warning"
    "Severity"             = "Warning"
    "RuleSuppressionID"    = "MyRuleSuppressionID"
    "SuggestedCorrections" = $suggestedCorrections
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
                    if ($asAst.Right.ToString() -eq '[system.security.principal.windowsbuiltinrole]::administrator')
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

More examples can be found in *Tests\Engine\CommunityAnalyzerRules*
