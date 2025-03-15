---
description: Null Comparison
ms.date: 12/03/2024
ms.topic: reference
title: PossibleIncorrectComparisonWithNull
---
# PossibleIncorrectComparisonWithNull

**Severity Level: Warning**

## Description

To ensure that PowerShell performs comparisons correctly, the `$null` element should be on the left
side of the operator.

There are multiple reasons why this occurs:

- `$null` is a scalar value. When the value on the left side of an operator is a scalar, comparison
  operators return a **Boolean** value. When the value is a collection, the comparison operators
  return any matching values or an empty array if there are no matches in the collection.
- PowerShell performs type casting on the right-hand operand, resulting in incorrect comparisons
  when `$null` is cast to other scalar types.

The only way to reliably check if a value is `$null` is to place `$null` on the left side of the
operator so that a scalar comparison is performed.

## How

Move `$null` to the left side of the comparison.

## Example

### Wrong

```powershell
function Test-CompareWithNull
{
    if ($DebugPreference -eq $null)
    {
    }
}
```

### Correct

```powershell
function Test-CompareWithNull
{
    if ($null -eq $DebugPreference)
    {
    }
}
```

## Try it Yourself

```powershell
# This example returns 'false' because the comparison does not return any objects from the array
if (@() -eq $null) { 'true' } else { 'false' }
# This example returns 'true' because the array is empty
if ($null -ne @()) { 'true' } else { 'false' }
```

This is how the comparison operator works by-design. But, as demonstrated, this can lead
to non-intuitive behavior, especially when the intent is simple test for null.

The following example demonstrates the designed behavior of the comparison operator when the
left-hand side is a collection. Each element in the collection is compared to the right-hand side
value. When true, that element of the collection is returned.

```powershell
PS> 1,2,3,1,2 -eq $null
PS> 1,2,3,1,2 -eq 1
1
1
PS> (1,2,3,1,2 -eq $null).count
0
PS> (1,2,$null,3,$null,1,2 -eq $null).count
2
```
