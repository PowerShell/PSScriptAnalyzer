#UseShouldProcessForStateChangingFunctions
**Severity Level: Warning**

##Description
Functions whose verbs change system state should support ```ShouldProcess```.

Verbs that should support ```ShouldProcess```:
	- ```New```
	- ```Reset```
	- ```Restart```
	- ```Set```
	- ```Start```
	- ```Stop```

##How to Fix
Include the attribute ```SupportsShouldProcess```, in the ```CmdletBindingBinding```.

##Example
###Wrong:
``` PowerShell
	function Set-ServiceObject
	{
	    [CmdletBinding()]
		param 
		(
			[string]
			$Parameter1
		)
		...
	}
```

###Correct: 
``` PowerShell
	function Set-ServiceObject
	{
	    [CmdletBinding(SupportsShouldProcess = $true)]
	    param 
		(
			[string]
			$Parameter1
		)
		...
	}
```
