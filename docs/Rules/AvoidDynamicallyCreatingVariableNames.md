---
description: Avoid dynamic variable names, instead use a hash table or similar dictionary type.
ms.date: 04/21/2026
ms.topic: reference
title: AvoidDynamicallyCreatingVariableNames
---
# AvoidDynamicallyCreatingVariableNames

**Severity Level: Information**

## Description

Don't create variables with dynamic names. It also makes the code difficult to understand and can
lead to unexpected behavior if the variable names are not unique or if they collide with existing
variables. A dynamic name is a name constructed using string concatenation or interpolation.
This rule checks for the use of `New-Variable` with a dynamic name.

> [!NOTE]
> This rule is not enabled by default. The user needs to enable it through settings.

## How to Fix

Use a hash table or similar dictionary type to store values with dynamic keys. When you require a
specific scope, option, or visibility, put the dictionary (hashtable) in that scope and apply the
appropriate option or visibility.

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

In this example, you want the values to be read-only and available in the script scope.
Put the hashtable in the script scope and make it read-only.

```powershell
New-Variable -Name My -Value @{} -Option ReadOnly -Scope Script
'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
    $Script:My[$_] = $i++
}
$Script:My.Two # returns 2
```

## Configuration

```powershell
Rules = @{
    PSAvoidDynamicallyCreatingVariableNames = @{
        Enable = $true
    }
}
```

### Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.

## References
- [New-Variable](xref:Microsoft.PowerShell.Utility.New-Variable)

