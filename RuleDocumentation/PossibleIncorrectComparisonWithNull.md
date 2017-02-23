# PossibleIncorrectComparisonWithNull
**Severity Level: Warning**

## Description
To ensure that PowerShell performs comparisons correctly, the `$null` element should be on the left side of the operator.

There are a number of reasons why this should occur:
* When there is an array on the left side of a null equality comparison, PowerShell will check for a `$null` IN the array rather than if the array is null.
* PowerShell will perform type casting left to right, resulting in incorrect comparisons when `$null` is cast to other types.

## How
Move `$null` to the left side of the comparison.

## Example
### Wrong?
``` PowerShell
function Test-CompareWithNull
{
	if ($DebugPreference -eq $null)
	{
	}
}
```

### Correct
``` PowerShell
function Test-CompareWithNull
{
	if ($null -eq $DebugPreference)
	{
	}
}
```
