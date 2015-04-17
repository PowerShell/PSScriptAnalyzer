#UseShouldProcessForStateChangingFunctions
**Severity Level: Warning**

##Description

Functions that have verbs like New, Start, Stop, Set, Reset that change system state should support 'ShouldProcess'

##How to Fix

To fix a violation of this rule, please add attribute SupportsShouldProcess. eg: [CmdletBinding(SupportsShouldProcess = $true)] to the function.

##Example

Wrong:
```
	function Get-ServiceObject
	{
	    [CmdletBinding()]
    	    param ([string]$c)
	}
```

Correct: 
```
	function Get-ServiceObject
	{
	    [CmdletBinding(SupportsShouldProcess = $true)]
	    param ([string]$c)
	}
```
