---
description: Reserved parameters
ms.date: 06/08/2026
ms.topic: reference
title: ReservedParams
---
# ReservedParams

**Severity Level: Error**

## Description

This rule detects when you attempt to redefine [common parameters][01] in an advanced function. When
you use the `CmdletBinding` or `Parameter` attributes, you're creating an advanced function. Common
parameters are automatically available in advanced functions, so you can't redefine them. If you're
trying to use a parameter name that conflicts with a common parameter, you need to change the name
of your parameter to something else.

## Example

### Noncompliant

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

### Compliant

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
