---
description: Avoid using ArrayList
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingArrayList
---
# AvoidUsingArrayList

**Severity Level: Warning**

**Default state: Disabled**

## Description

Avoid the **ArrayList** class for new development. The documentation for the [ArrayList class][01]
recommends using the [System.Collections.Generic.List\<T\>][02] class instead.

The **ArrayList** class is a non-generic collection that can hold objects of any type. The generic
**List\<T\>** class provides better performance, type safety, and the `List<T>.Add()` method doesn't
have the output side-effects that `ArrayList.Add()` has.

In cases where only the `ArrayList.Add()` method is used, you could replace the **ArrayList** class
with a generic `List[Object]` class or consider using the more idiomatic PowerShell pipeline syntax
instead.

## Example

### Noncompliant

```powershell
# Using an ArrayList
$List = [System.Collections.ArrayList]::new()
1..3 | ForEach-Object { $List.Add($_) } # Note that this will return the index of the added element
```

### Compliant

```powershell
# Using a generic List
$List = [System.Collections.Generic.List[Object]]::new()
1..3 | ForEach-Object { $List.Add($_) } # This will not return anything
```

```powershell
# Creating a fixed array by using the PowerShell pipeline
$List = 1..3 | ForEach-Object { $_ }
```

## Configure rule

```powershell
Rules = @{
    PSAvoidUsingArrayList = @{
        Enable = $true
    }
}
```

### Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.

<!-- link references -->
[01]: xref:System.Collections.ArrayList#remarks
[02]: xref:System.Collections.Generic.List%601