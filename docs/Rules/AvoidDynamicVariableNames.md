---
description: Avoid dynamic variable names, instead use a hash table or similar dictionary type.
ms.date: 04/21/2026
ms.topic: reference
title: AvoidDynamicVariableNames
---
# AvoidDynamicVariableNames

**Severity Level: Warning**

## Description

Do not dynamically create variable names in the general variable pool, this might introduce conflicts with other
variables and is difficult to maintain.

## How

Use a hash table or similar dictionary type to store values with dynamic keys.

## Example

### Wrong

```powershell
'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
    New-Variable -Name "My$_" -Value ($i++)
}
$MyTwo # returns 2
```

### Correct

```powershell
$My = @{}
'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
    $My[$_] = $i++
}
$My.Two # returns 2
```

When it concerns a specific scope, option or visibility, put the concerned dictionary (hash table) in that
scope, option or visibility. In example, if the values should be read only and available in the script scope,
put the _hash table_ in the script scope and make it read only.:

```powershell
New-Variable -Name My -Value @{} -Option ReadOnly -Scope Script
'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
    $Script:My[$_] = $i++
}
$Script:My.Two # returns 2
```