---
description: Use single cmdlet nouns
ms.date: 07/23/2026
ms.topic: reference
title: UseSingularNouns
---
# UseSingularNouns

**Severity Level: Warning**

**Default state: Enabled**

## Description

This rule detects cmdlet names that use plural nouns instead of singular nouns.

PowerShell best practices require that cmdlets use singular nouns, not plurals. You can use the
`NounAllowList` parameter to exclude specific nouns from this rule, or suppress the rule for
individual functions as described in the Suppression section. If a violation is found, change
the plural noun to its singular form.

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

## Configure rule

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

This parameter specifies which command nouns to exclude from this rule. It accepts a string array.
Both `Data` and `Windows` are common false positives and excluded by default. Default values are
`'Data'` and `'Windows'`.

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '')]
```

To suppress the rule for a single function, pass the function name as the second argument. The
name isn't case sensitive. This is useful when the attribute is applied at script or module scope
and covers more than one function.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', 'Test-Scripts')]
param()

function Test-Scripts {}
```
