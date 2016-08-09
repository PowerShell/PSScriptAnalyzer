#UseShouldProcessCorrectly 
**Severity Level: Warning**

##Description
Checks that if the ```SupportsShouldProcess``` is present, i.e, ```[CmdletBinding(SupportsShouldProcess = $true)]```, and then tests if the function or CMDLet calls 
ShouldProcess or ShouldContinue; i.e ```$PSCmdlet.ShouldProcess``` or ```$PSCmdlet.ShouldContinue```.

A violation is any function where ```SupportsShouldProcess``` that makes no calls to ```ShouldProcess``` or ```ShouldContinue```.

Scripts with one or the other but not both will generally run into an error or unexpected behavior.

##How to Fix
To fix a violation of this rule, please call ```ShouldProcess``` method in advanced functions when ```SupportsShouldProcess``` argument is present. 
Or please add ```SupportsShouldProcess``` argument when calling ```ShouldProcess```.
You can get more details by running ```Get-Help about_Functions_CmdletBindingAttribute``` and ```Get-Help about_Functions_Advanced_Methods``` command in Windows PowerShell.

##Example
###Wrong： 
``` PowerShell
	function Set-File
	{
	    [CmdletBinding(SupportsShouldProcess=$true)]
	    Param
	    (
	        # Path to file
			[Parameter(Mandatory=$true)] 
	        $Path
	    )

	    Begin
	    {
	    }
	    Process
	    {
			"String" | Out-File -FilePath $FilePath
	    }
	    End
	    {
	    }
	}
```

###Correct:
``` PowerShell
	function Set-File
	{
	    [CmdletBinding(SupportsShouldProcess=$true)]
	    Param
	    (
	        # Path to file
			[Parameter(Mandatory=$true)] 
	        $Path
	    )

	    Begin
	    {
	    }
	    Process
	    {
			if ($PSCmdlet.ShouldProcess("Target", "Operation"))
	        {
				"String" | Out-File -FilePath $FilePath
			}
	    }
	    End
	    {
			if ($pscmdlet.ShouldContinue("Yes", "No")) 
			{
				...
        	}
	    }
	}
```
