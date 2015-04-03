#AvoidGlobalVars 
**Severity Level: Warning**

##Description

A variable is a unit of memory in which values are stored. Windows PowerShell controls access to variables, functions, aliases, and drives through a mechanism known as scoping. Variables and functions that are present when Windows PowerShell starts have been created in the global scope. This includes automatic variables and preference variables. This also includes the variables, aliases, and functions that are in your Windows PowerShell profiles.A variable is a unit of memory in which values are stored. Windows PowerShell controls access to variables, functions, aliases, and drives through a mechanism known as scoping. Variables and functions that are present when Windows PowerShell starts have been created in the global scope. This includes automatic variables and preference variables. This also includes the variables, aliases, and functions that are in your Windows PowerShell profiles.

##How to Fix

To fix a violation of this rule, please consider to use other scope modifiers.

##Example
Wrong:

	$Global:var1 = $null
	function NotGlobal ($var)
	{
    	$a = $var + $var1
	}

Correct:

	$Global:var1 = $null
	function NotGlobal ($var1,$var2)
	{
    	$a = $var1 + $var2
	}
