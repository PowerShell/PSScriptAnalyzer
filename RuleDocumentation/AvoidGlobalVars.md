#AvoidGlobalVars 
**Severity Level: Warning**

##Description
A variable is a unit of memory in which values are stored. Windows PowerShell controls access to variables, functions, aliases, and drives through a mechanism known as scoping. 
Variables and functions that are present when Windows PowerShell starts have been created in the global scope. 

Globally scoped variables include:
	- automatic variables
	- preference variables
	- variables, aliases, and functions that are in your Windows PowerShell profiles

To understand more about scoping, see ```Get-Help about_Scopes```.

##How to Fix
Use other scope modifiers for variables.

##Example
###Wrong:
``` PowerShell
$Global:var1 = $null
function NotGlobal ($var)
{
	$a = $var + $var1
}
```

###Correct:
``` PowerShell
$var1 = $null
function NotGlobal ($var1, $var2)
{
		$a = $var1 + $var2
}
```
