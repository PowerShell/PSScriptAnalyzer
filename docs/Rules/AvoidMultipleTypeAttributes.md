---
description: Avoid multiple type specifiers on parameters.
ms.date: 06/28/2023
ms.topic: reference
title: AvoidMultipleTypeAttributes
---
# AvoidMultipleTypeAttributes

**Severity Level: Warning**

## Description

Parameters should not have more than one type specifier. Multiple type specifiers on parameters
can cause runtime errors.

## How

Ensure each parameter has only 1 type specifier.

## Example

### Wrong

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [switch]
        [int]
        $Switch
    )
}
```

### Correct

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [switch]
        $Switch
    )
}
```
