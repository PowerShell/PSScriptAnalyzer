# PossibleIncorrectComparisonWithNull

**Severity Level: Warning**

## Description

To ensure that PowerShell performs comparisons correctly, the `$null` element should be on the left side of the operator.

There are a number of reasons why this should occur:
* `$null` is a scalar. When the input (left side) to an operator is a scalar value, comparison operators return a Boolean value. When the input is a collection of values, the comparison operators return any matching values, or an empty array if there are no matches in the collection. The only way to reliably check if a value is `$null` is to place `$null` on the left side of the operator so that a scalar comparison is perfomed.
* PowerShell will perform type casting left to right, resulting in incorrect comparisons when `$null` is cast to other scalar types.

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

## Try it Yourself

``` PowerShell
if (@() -eq $null) { 'true' } else { 'false' }  # Returns false
if ($null -ne @()) { 'true' } else { 'false' }  # Returns true
```
