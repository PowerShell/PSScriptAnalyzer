#AvoidUninitializedVariable
**Severity Level: Warning**

##Description
A variable is a unit of memory in which values are stored. Windows PowerShell controls access to variables, functions, aliases, and drives through a mechanism known as scoping. 

All non-global variables must be initialized, otherwise potential bugs could be introduced.

##How to Fix
Initialize non-global variables.

##Example
###Wrong：    
``` PowerShell
function NotGlobal {
	$localVars = "Localization?"
	$uninitialized
	Write-Output $uninitialized
}

###Correct:   
``` PowerShell
function NotGlobal {
	$localVars = "Localization?"
	Write-Output $localVars
}
```
