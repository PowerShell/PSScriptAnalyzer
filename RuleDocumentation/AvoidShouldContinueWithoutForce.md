#AvoidShouldContinueWithoutForce
**Severity Level: Warning**

##Description
Functions that use ShouldContinue should have a boolean force parameter to allow user to bypass it.

##How to Fix
Call the ```ShouldContinue``` method in advanced functions when ```ShouldProcess``` method returns ```$true```. 

You can get more details by running ```Get-Help about_Functions_CmdletBindingAttribute``` and ```Get-Help about_Functions_Advanced_Methods``` command in Windows PowerShell.

##Example
###Wrong:
``` PowerShell 
Function Test-ShouldContinue
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param
    (
        $MyString = 'blah'
    )

    if ($PsCmdlet.ShouldContinue("ShouldContinue Query", "ShouldContinue Caption")) 
	{
        ...
    }
}
```

###Correct:
``` PowerShell
Function Test-ShouldContinue
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param
    (
        $MyString = 'blah',
        [Switch]$Force
    )

    if ($PsBoundParameters.ContainsKey('force') -or $PsCmdlet.ShouldContinue("ShouldContinue Query", "ShouldContinue Caption")) 
	{
        ...
    }
}
```
