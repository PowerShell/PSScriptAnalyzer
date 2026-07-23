---
description: Avoid using empty catch block
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingEmptyCatchBlock
---
# AvoidUsingEmptyCatchBlock

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects empty `catch` blocks. Empty catch blocks are problematic because they silently
suppress exceptions without any error handling, logging, or recovery action. This can mask bugs and
make troubleshooting difficult. Always handle exceptions explicitly by using `Write-Error` to log
the error, `throw` to propagate it, or provide appropriate recovery logic within the catch block.

## Example

### Noncompliant

```powershell
try
{
    1/0
}
catch [DivideByZeroException]
{
}
```

### Compliant

```powershell
try
{
    1/0
}
catch [DivideByZeroException]
{
    Write-Error 'DivideByZeroException'
}

try
{
    1/0
}
catch [DivideByZeroException]
{
    throw 'DivideByZeroException'
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