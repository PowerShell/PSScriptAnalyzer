# ProvideDefaultParameterValue
**Severity Level: Warning**

## Description
Just like non-global scoped variables, parameters must have a default value if they are not mandatory, i.e `Mandatory=$false`.
Having optional parameters without default values leads to uninitialized variables leading to potential bugs.

## How
Specify a default value for all parameters that are not mandatory.

## Example
### Wrong
``` PowerShell
function Test($Param1)
{
	$Param1
}
```

### Correct
``` PowerShell
function Test($Param1 = $null)
{
	$Param1
}
```
