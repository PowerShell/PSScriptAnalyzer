---
description: Reserved Parameters
ms.date: 03/06/2024
ms.topic: reference
title: ReservedParams
---
# ReservedParams

**Severity Level: Error**

## Description

You can't redefine [common parameters][01] in an advanced function. Using the `CmdletBinding` or
`Parameter` attributes creates an advanced function. The common parameters are are automatically
available in advanced functions, so you can't redefine them.

## How

Change the name of the parameter.

## Example

### Wrong

```powershell
function Test
{
    [CmdletBinding()]
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
    [CmdletBinding()]
    Param
    (
        $Err,
        $Parameter2
    )
}
```

[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_commonparameters
