---
description: Use the same pattern when defining parameters
ms.date: 06/10/2026
ms.topic: reference
title: UseConsistentParametersKind
---
# UseConsistentParametersKind

**Severity Level: Warning**

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
