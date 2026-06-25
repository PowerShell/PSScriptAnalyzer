---
description: Avoid using empty catch block
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingEmptyCatchBlock
---
# AvoidUsingEmptyCatchBlock

**Severity Level: Warning**

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
