---
description: Comparison with null
ms.date: 06/05/2026
ms.topic: reference
title: PossibleIncorrectComparisonWithNull
---
# PossibleIncorrectComparisonWithNull

**Severity Level: Warning**

## Description

This rule detects comparisons where `$null` isn't on the left side of the comparison operator. To
ensure that PowerShell performs comparisons correctly, the `$null` element should be on the left
side of the operator.

There are multiple reasons why this placement matters:

- `$null` is a scalar value. When the value on the left side of an operator is a scalar, comparison
  operators return a **Boolean** value. When the value is a collection, the comparison operators
  return any matching values or an empty array if there aren't any matches in the collection.
- PowerShell performs type casting on the right-hand operand, resulting in incorrect comparisons
  when `$null` is cast to other scalar types.

The only way to reliably check if a value is `$null` is to place `$null` on the left side of the
operator so that a scalar comparison is performed.

### Comparison operator behavior

The comparison operator works by design in the following way.

```powershell
# This example returns 'false' because the comparison doesn't return any objects from the array
if (@() -eq $null) { 'true' } else { 'false' }

# This example returns 'true' because the array is empty
if ($null -ne @()) { 'true' } else { 'false' }
```

This behavior can produce unexpected results, especially when you intend to perform a null check.

The following example demonstrates how comparison operators behave when the left-hand side is a
collection. The operator compares each element in the collection to the right-hand side value and
returns only the matching elements from the collection.

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

## Example

### Noncompliant

```powershell
function Test-CompareWithNull
{
    if ($DebugPreference -eq $null)
    {
    }
}
```

### Compliant

```powershell
function Test-CompareWithNull
{
    if ($null -eq $DebugPreference)
    {
    }
}
```
