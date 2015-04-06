#AvoidUninitializedVariable
**Severity Level: Warning**


##Description

A variable is a unit of memory in which values are stored. Windows PowerShell controls access to variables, functions, aliases, and drives through a mechanism known as scoping. The scope of an item is another term for its visibility. Non-global variables must be initialized. 


##How to Fix

To fix a violation of this rule, please initialize non-global variables.

##Example

Wrong：    

	function NotGlobal {
	    $localVars = "Localization?"
	    $unitialized
	    Write-Output $unitialized
	}


Correct:   

	function NotGlobal {
	    $localVars = "Localization?"
	    Write-Output $localVars
	}