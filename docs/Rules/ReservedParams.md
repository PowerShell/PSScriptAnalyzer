---
description: Reserved Parameters
ms.custom: PSSA v1.20.0
ms.date: 10/18/2021
ms.topic: reference
title: ReservedParams
---
# ReservedParams

**Severity Level: Error**

## Description

You cannot use reserved common parameters in an advanced function.

## How

Change the name of the parameter.

## Example

### Wrong

```powershell
function Test
{
    [CmdletBinding]
    Param
    (
        $ErrorVariable,
        $Parameter2
    )
}
```

### Correct

```powershell
function Test
{
    [CmdletBinding]
    Param
    (
        $Err,
        $Parameter2
    )
}
```
