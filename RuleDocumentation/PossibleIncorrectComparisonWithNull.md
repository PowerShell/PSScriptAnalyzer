# PossibleIncorrectComparisonWithNull

**Severity Level: Warning**

## Description

To ensure that PowerShell performs comparisons correctly, the `$null` element should be on the left side of the operator.

There are a number of reasons why this should occur:
* `$null` is a scalar. When the input (left side) to an operator is a scalar value, comparison operators return a Boolean value. When the input is a collection of values, the comparison operators return any matching values, or an empty array if there are no matches in the collection. The only way to reliably check if a value is `$null` is to place `$null` on the left side of the operator so that a scalar comparison is performed.
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
# Both expressions below return 'false' because the comparison does not return an object and therefore the if statement always falls through:
if (@() -eq $null) { 'true' } else { 'false' }
if (@() -ne $null) { 'true' }else { 'false' } 
```
This is just the way how the comparison operator works (by design) but as demonstrated this can lead to unintuitive behaviour, especially when the intent is just a null check. The following example demonstrated the designed behaviour of the comparison operator, whereby for each element in the collection, the comparison with the right hand side is done, and where true, that element in the collection is returned.
``` PowerShell
PS> 1,2,3,1,2 -eq $null
PS> 1,2,3,1,2 -eq 1    
1
1
PS> (1,2,3,1,2 -eq $null).count            
0
PS> (1,2,$null,3,$null,1,2 -eq $null).count
2
```
