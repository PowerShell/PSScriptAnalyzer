---
description: Use single cmdlet nouns
ms.date: 06/11/2026
ms.topic: reference
title: UseSingularNouns
---
# UseSingularNouns

**Severity Level: Warning**

## Description

This rule detects cmdlet names that use plural nouns instead of singular nouns.

PowerShell best practices require that cmdlets use singular nouns, not plurals. You can use the
`NounAllowList` parameter to exclude specific nouns from this rule, or suppress the rule for
individual functions using `SuppressMessageAttribute`. If a violation is found, change the plural
noun to its singular form.

For example:

```
function Get-Elements {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', 'Get-Elements')]
    Param()
}
```

## Example

### Noncompliant

```powershell
function Get-Files
{
    ...
}
```

### Compliant

```powershell
function Get-File
{
    ...
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

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To disable this rule, set this parameter to `$false`. The default value is `$true`.

### NounAllowList

This parameter specifies which noun commands to exclude from this rule. It accepts a string value.
Both `Data` and `Windows` are common false positives and excluded by default. Default values are
`'Data'` and `'Windows'`.
