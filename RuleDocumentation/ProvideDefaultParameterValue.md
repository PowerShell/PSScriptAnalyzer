#ProvideDefaultParameterValue
**Severity Level: Warning**


##Description
Parameters must have a default value as uninitialized parameters will lead to potential bugs in the scripts.

##How to Fix

To fix a violation of this rule, please specify a default value for all parameters

##Example

Wrong： 

```
function Test($Param1)
{
	$Param1
}
```

Correct: 

```
ffunction Test($Param1 = $null)
{
	$Param1
}
```
