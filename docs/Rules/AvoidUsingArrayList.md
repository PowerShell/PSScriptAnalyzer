---
description: Avoid using ArrayList
ms.date: 04/16/2025
ms.topic: reference
title: AvoidUsingArrayList
---
# AvoidUsingArrayList

**Severity Level: Warning**

## Description

Per dotnet best practices, the
[`ArrayList` class](https://learn.microsoft.com/dotnet/api/system.collections.arraylist)
is not recommended for new development, the same recommendation applies to PowerShell:

Avoid the ArrayList class for new development.
The `ArrayList` class is a non-generic collection that can hold objects of any type. This is inline with the fact
that PowerShell is a weakly typed language. However, the `ArrayList` class does not provide any explicit type
safety and performance benefits of generic collections. Instead of using an `ArrayList`, consider using either a
[`System.Collections.Generic.List[Object]`](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)
class or a fixed PowerShell array.
Besides, the `ArrayList.Add` method returns the index of the added element which often unintentionally pollutes the
PowerShell pipeline and therefore might cause unexpected issues.

## How to Fix

In cases where only the `Add` method is used, you might just replace the `ArrayList` class with a generic
`List[Object]` class but you could also consider using the idiomatic PowerShell pipeline syntax instead.

## Example

### Wrong

```powershell
# Using an ArrayList
$List = [System.Collections.ArrayList]::new()
1..3 | ForEach-Object { $List.Add($_) } # Note that this will return the index of the added element
```

### Correct

```powershell
# Using a generic List
$List = [System.Collections.Generic.List[Object]]::new()
1..3 | ForEach-Object { $List.Add($_) } # This will not return anything
```

```PowerShell
# Creating a fixed array by using the PowerShell pipeline
$List = 1..3 | ForEach-Object { $_ }
```