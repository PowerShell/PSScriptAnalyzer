---
description: Avoid reserved words as function names
ms.date: 08/31/2025
ms.topic: reference
title: AvoidReservedWordsAsFunctionNames
---
# AvoidReservedWordsAsFunctionNames

**Severity Level: Warning**

## Description

Avoid using reserved words as function names. Using reserved words as function names can cause
errors or unexpected behavior in scripts.

## How to Fix

Avoid using any of the reserved words as function names. Choose a different name that's not a
reserved word.

See [about_Reserved_Words][01] for a list of reserved words in PowerShell.

## Example

### Wrong

```powershell
# Function is a reserved word
function function {
    Write-Host "Hello, World!"
}
```

### Correct

```powershell
# myFunction is not a reserved word
function myFunction {
    Write-Host "Hello, World!"
}
```

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_reserved_words
