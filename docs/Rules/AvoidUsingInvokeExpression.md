---
description: Avoid Using Invoke-Expression
ms.date: 06/28/2023
ms.topic: reference
title: AvoidUsingInvokeExpression
---
# AvoidUsingInvokeExpression

**Severity Level: Warning**

## Description

Care must be taken when using the `Invoke-Expression` command. The `Invoke-Expression` executes the
specified string and returns the results.

Code injection into your application or script can occur if the expression passed as a string
includes any data provided from the user.

## How

Remove the use of `Invoke-Expression`.

## Example

### Wrong

```powershell
Invoke-Expression 'Get-Process'
```

### Correct

```powershell
Get-Process
```
