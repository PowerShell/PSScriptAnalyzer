---
description: Cmdlet Singular Noun
ms.date: 03/27/2024
ms.topic: reference
title: UseSingularNouns
---
# UseSingularNouns

**Severity Level: Warning**

## Description

PowerShell team best practices state cmdlets should use singular nouns and not plurals. Suppression
allows you to suppress the rule for specific function names. For example:

```
function Get-Elements {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', 'Get-Elements')]
    Param()
}
```

## Configuration

```powershell
Rules = @{
    PSUseSingularNouns = @{
        Enable           = $true
        NounAllowList    = 'Data', 'Windows', 'Foos'
    }
}
```

### Parameters

- `Enable`: `bool` (Default value is `$true`)

  Enable or disable the rule during ScriptAnalyzer invocation.

- `NounAllowList`: `string[]` (Default value is `{'Data', 'Windows'}`)

  Commands to be excluded from this rule. `Data` and `Windows` are common false positives and are
  excluded by default.

## How

Change plurals to singular.

## Example

### Wrong

```powershell
function Get-Files
{
    ...
}
```

### Correct

```powershell
function Get-File
{
    ...
}
```
