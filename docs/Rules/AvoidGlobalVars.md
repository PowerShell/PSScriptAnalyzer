---
description: No Global Variables
ms.date: 06/28/2023
ms.topic: reference
title: AvoidGlobalVars
---
# AvoidGlobalVars

**Severity Level: Warning**

## Description

A variable is a unit of memory in which values are stored. PowerShell controls access to variables,
functions, aliases, and drives through a mechanism known as scoping. Variables and functions that
are present when PowerShell starts have been created in the global scope.

Globally scoped variables include:

- Automatic variables
- Preference variables
- Variables, aliases, and functions that are in your PowerShell profiles

To understand more about scoping, see `Get-Help about_Scopes`.

## How

Use other scope modifiers for variables.

## Example

### Wrong

```powershell
$Global:var1 = $null
function Test-NotGlobal ($var)
{
    $a = $var + $var1
}
```

### Correct

```powershell
$var1 = $null
function Test-NotGlobal ($var1, $var2)
{
    $a = $var1 + $var2
}
```
