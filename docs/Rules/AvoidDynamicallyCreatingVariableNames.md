---
description: Avoid dynamic variable names, instead use a hash table or similar dictionary type.
ms.date: 07/21/2026
ms.topic: reference
title: AvoidDynamicallyCreatingVariableNames
---
# AvoidDynamicallyCreatingVariableNames

**Severity Level: Information**

**Default state: Disabled**

## Description

This rule checks for the use of `New-Variable` with a dynamic name. Don't create variables with
dynamic names. A dynamic name is a name constructed using string concatenation or interpolation.
Dynamic names make the code difficult to understand and can lead to unexpected behavior if the
variable names aren't unique.

Use a hash table or similar dictionary type to store values with dynamic keys. If you require a
specific scope, option, or visibility, put the dictionary (hashtable) in that scope and apply the
appropriate option or visibility.

## Example

### Noncompliant

```powershell
'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
    New-Variable -Name "My$_" -Value ($i++)
}
$MyTwo # returns 2
```

### Compliant

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

## Configure rule

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

- [New-Variable][02]
- [about_Scopes][01]

<!-- link references -->
[01]: /powershell/modules/microsoft.powershell.core/about/about_scopes
[02]: xref:Microsoft.PowerShell.Utility.New-Variable
