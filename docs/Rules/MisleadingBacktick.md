---
description: Misleading backtick
ms.date: 07/21/2026
ms.topic: reference
title: MisleadingBacktick
---
# MisleadingBacktick

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects lines where a trailing backtick is followed by one or more whitespace characters.
A trailing backtick is used for line continuation only when it's the last character on the line. If
whitespace follows the backtick on the same line the continuation doesn't work as intended, which
can make the code look valid but behave unexpectedly.

## Example

### Noncompliant

```powershell
# The line in this example ends with a backtick and trailing whitespace
Get-Process `
| Where-Object CPU -gt 100
```

### Compliant

```powershell
Get-Process |
    Where-Object CPU -gt 100
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
