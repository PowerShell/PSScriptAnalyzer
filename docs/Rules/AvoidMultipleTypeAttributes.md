---
description: Avoid multiple type specifiers on parameters
ms.date: 07/21/2026
ms.topic: reference
title: AvoidMultipleTypeAttributes
---
# AvoidMultipleTypeAttributes

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[02]: ../using-scriptanalyzer.md