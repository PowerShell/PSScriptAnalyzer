---
description: Avoid reserved words as function names
ms.date: 06/01/2026
ms.topic: reference
title: AvoidReservedWordsAsFunctionNames
---
# AvoidReservedWordsAsFunctionNames

**Severity Level: Warning**

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

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_reserved_words
