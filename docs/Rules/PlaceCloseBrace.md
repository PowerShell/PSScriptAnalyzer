---
description: Place close braces consistently
ms.date: 07/21/2026
ms.topic: reference
title: PlaceCloseBrace
---
# PlaceCloseBrace

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule detects closing braces (`}`) that aren't placed on a new line by themselves. When
`NoEmptyLineBefore` is enabled, it also detects empty lines immediately before closing braces.

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

## Configure rule

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
