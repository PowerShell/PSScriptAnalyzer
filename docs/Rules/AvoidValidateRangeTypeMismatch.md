---
description: Detect integer ValidateRange boundaries on floating-point and decimal parameters
ms.topic: reference
title: AvoidValidateRangeTypeMismatch
---
# AvoidValidateRangeTypeMismatch

**Severity Level: Warning**

**Default state: Always enabled**

## Description

Integer boundaries in a `ValidateRange` attribute can select integer validation behavior for
floating-point and decimal parameters. This can allow fractional values outside the declared range.
Use boundaries with the same numeric type as the parameter.

## Example

### Noncompliant

```powershell
function Set-Percentage
{
    param([double][ValidateRange(0, 100)]$Value)
}
```

### Compliant

```powershell
function Set-Percentage
{
    param([double][ValidateRange(0.0, 100.0)]$Value)
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][01].

<!-- Link references -->
[01]: ../using-scriptanalyzer.md#suppressing-rules
