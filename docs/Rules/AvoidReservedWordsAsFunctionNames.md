---
description: Avoid reserved words as function names
ms.date: 08/31/2025
ms.topic: reference
title: AvoidReservedWordsAsFunctionNames
---
# AvoidReservedWordsAsFunctionNames

**Severity Level: Warning**

## Description

Avoid using reserved words as function names. Using reserved words as function
names can cause errors or unexpected behavior in scripts.

## How to Fix

Avoid using any of the reserved words as function names. Instead, choose a
different name that is not reserved.

See [`about_Reserved_Words`](https://learn.microsoft.com/en-gb/powershell/module/microsoft.powershell.core/about/about_reserved_words) for a list of reserved
words in PowerShell.

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