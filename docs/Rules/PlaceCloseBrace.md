---
description: Place close braces consistently
ms.date: 06/05/2026
ms.topic: reference
title: PlaceCloseBrace
---
# PlaceCloseBrace

**Severity Level: Warning**

## Description

This rule detects closing braces (`}`) that aren't placed on a new line by themselves or that are
followed by empty lines. Close brace placement should follow a consistent style. It should be on a
new line by itself and shouldn't be followed by an empty line. This rule is **disabled** by default.

## Example

### Noncompliant

```powershell
if ($true) {
    'example' }
Get-Process
```

### Compliant

```powershell
if ($true) {
    'example'
}
Get-Process
```

## Configuration

```powershell
Rules = @{
    PSPlaceCloseBrace = @{
        Enable = $true
        NoEmptyLineBefore = $false
        IgnoreOneLineBlock = $true
        NewLineAfter = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### NoEmptyLineBefore

This parameter controls whether ScriptAnalyzer checks the code for an empty line before a close
brace. It accepts a boolean value. To enable this check, set this parameter to `$true`. The default
value is `$false`.

### IgnoreOneLineBlock

This parameter controls whether ScriptAnalyzer skips one-line blocks when checking the code against
this rule. It accepts a boolean value. To disable skipping one-line blocks, set this parameter to
`$false`. The default value is `$true`.

### NewLineAfter

This parameter controls whether ScriptAnalyzer checks that a new line follows a closing brace. It
accepts a boolean value. To disable this check, set this parameter to `$false`. The default value is
`$true`.
