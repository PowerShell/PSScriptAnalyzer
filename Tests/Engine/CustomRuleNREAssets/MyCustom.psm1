<#
.SYNOPSIS
    Static methods are not allowed in constrained language mode.
.DESCRIPTION
    Static methods are not allowed in constrained language mode.
    To fix a violation of this rule, use a cmdlet or function instead of a static method.
.EXAMPLE
    Test-StaticMethod -CommandAst $CommandAst
.INPUTS
    [System.Management.Automation.Language.ScriptBlockAst]
.OUTPUTS
    [PSCustomObject[]]
.NOTES
    Reference: Output, CLM info.
#>
function Test-StaticMethod
{
    [CmdletBinding()]
    [OutputType([PSCustomObject[]])]
    Param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    Process
    {
        try
        {
            # Gets methods

            $invokedMethods = $ScriptBlockAst.FindAll({$args[0] -is [System.Management.Automation.Language.CommandExpressionAst] -and $args[0].Expression -match "^\[.*\]::" },$true)
            foreach ($invokedMethod in $invokedMethods)
            {
                [PSCustomObject]@{Message  = "Avoid Using Static Methods";
                                  Extent   = $invokedMethod.Extent;
                                  RuleName = $PSCmdlet.MyInvocation.InvocationName;
                                  Severity = "Warning"}
            }
        }
        catch
        {
            $PSCmdlet.ThrowTerminatingError($PSItem)
        }
    }
}
