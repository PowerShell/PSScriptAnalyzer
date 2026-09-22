---
description: Avoid reserved words as function names
ms.date: 07/21/2026
ms.topic: reference
title: AvoidReservedWordsAsFunctionNames
---
# AvoidReservedWordsAsFunctionNames

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects function names that use PowerShell reserved words. Using reserved words as
function names causes errors or unexpected behavior in PowerShell scripts. Choose function names
that don't conflict with any PowerShell reserved words.

See [about_Reserved_Words][01] for a list of reserved words in PowerShell.

## Example

### Noncompliant

```powershell
# Function is a reserved word
function function {
    Write-Host "Hello, World!"
}
```

### Compliant

```powershell
# myFunction is not a reserved word
function myFunction {
    Write-Host "Hello, World!"
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_reserved_words
[02]: ../using-scriptanalyzer.md
