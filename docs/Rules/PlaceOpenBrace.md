---
description: Place open braces consistently
ms.date: 06/05/2026
ms.topic: reference
title: PlaceOpenBrace
---
# PlaceOpenBrace

**Severity Level: Warning**

## Description

This rule detects opening braces (`{`) that don't follow a consistent style. Opening braces can be
required on the same line as the preceding keyword or on the next line, based on configuration. You
can also require a new line after the opening brace. This rule is **disabled** by default.

## Example

### Noncompliant

```powershell
if ($true)
{
    'example'
}
```

### Compliant

```powershell
if ($true) {
    'example'
}
```

## Configuration

```powershell
Rules = @{
    PSPlaceOpenBrace = @{
        Enable = $true
        OnSameLine = $true
        NewLineAfter = $true
        IgnoreOneLineBlock = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### OnSameLine

This parameter controls whether ScriptAnalyzer checks that an open brace is on the same line as the
preceding keyword. It accepts a boolean value. To disable this check, set this parameter to
`$false`. The default value is `$true`.

### NewLineAfter

This parameter controls whether ScriptAnalyzer checks that a new line follows an opening brace. It
accepts a boolean value. To disable this check, set this parameter to `$false`. The default value is
`$true`.

### IgnoreOneLineBlock

This parameter controls whether ScriptAnalyzer skips one-line blocks when checking the code against
this rule. It accepts a boolean value. To disable skipping one-line blocks, set this parameter to
`$false`. The default value is `$true`.
