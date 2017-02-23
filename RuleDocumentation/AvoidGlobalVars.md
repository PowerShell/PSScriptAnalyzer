# AvoidGlobalVars

**Severity Level: Warning**

## Description

A variable is a unit of memory in which values are stored. Windows PowerShell controls access to variables, functions, aliases, and drives through a mechanism known as scoping.
Variables and functions that are present when Windows PowerShell starts have been created in the global scope.

Globally scoped variables include:
* Automatic variables
* Preference variables
* Variables, aliases, and functions that are in your Windows PowerShell profiles

To understand more about scoping, see ```Get-Help about_Scopes```.

## How

Use other scope modifiers for variables.

## Example

### Wrong

``` PowerShell
$Global:var1 = $null
function Test-NotGlobal ($var)
{
	$a = $var + $var1
}
```

### Correct

``` PowerShell
$var1 = $null
function Test-NotGlobal ($var1, $var2)
{
		$a = $var1 + $var2
}
```
