---
description: Avoid Using Empty Catch Block
ms.custom: PSSA v1.20.0
ms.date: 10/18/2021
ms.topic: reference
title: AvoidUsingEmptyCatchBlock
---
# AvoidUsingEmptyCatchBlock

**Severity Level: Warning**

## Description

Empty catch blocks are considered a poor design choice because any errors occurring in a
`try` block cannot be handled.

## How

Use `Write-Error` or `throw` statements within the catch block.

## Example

### Wrong

```powershell
try
{
    1/0
}
catch [DivideByZeroException]
{
}
```

### Correct

```powershell
try
{
    1/0
}
catch [DivideByZeroException]
{
    Write-Error "DivideByZeroException"
}

try
{
    1/0
}
catch [DivideByZeroException]
{
    throw "DivideByZeroException"
}
```
