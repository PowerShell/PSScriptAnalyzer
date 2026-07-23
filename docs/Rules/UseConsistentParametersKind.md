---
description: Use the same pattern when defining parameters
ms.date: 07/23/2026
ms.topic: reference
title: UseConsistentParametersKind
---
# UseConsistentParametersKind

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule detects when functions don't consistently use the same pattern for defining parameters.
All functions should use the same parameter definition pattern to enforce consistent code style
across the codebase. Rewrite any function that doesn't match the configured parameter definition
kind. In simple scenarios, both the `Inline` and `ParamBlock` function definition patterns are
functionally equivalent.

## Example

### Noncompliant

```powershell
function f([Parameter()]$FirstParam) {
    return
}
```

### Compliant

```powershell
function g {
    param([Parameter()]$FirstParam)
    return
}
```

## Configure rule

```powershell
Rules = @{
    PSUseConsistentParametersKind  = @{
        Enable = $true
        ParametersKind = 'ParamBlock'
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### ParametersKind

Use this parameter to specify the type of parameter definition pattern that should be used
consistently across all functions. It accepts a string value, which can be either `Inline` or
`ParamBlock`. The default value is `ParamBlock`.
