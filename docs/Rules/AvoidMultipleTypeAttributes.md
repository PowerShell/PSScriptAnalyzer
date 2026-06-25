---
description: Avoid multiple type specifiers on parameters
ms.date: 06/01/2026
ms.topic: reference
title: AvoidMultipleTypeAttributes
---
# AvoidMultipleTypeAttributes

**Severity Level: Warning**

## Description

This rule detects parameters that have multiple type specifiers applied to them. Parameters
shouldn't have multiple type specifiers. When you apply more than one type attribute to a parameter,
it can lead to unexpected type coercion or runtime errors.

Each parameter should have exactly one type specifier to ensure predictable behavior and type
safety.

## Example

### Noncompliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [switch]
        [int]
        $Switch
    )
}
```

### Compliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [switch]
        $Switch
    )
}
```
