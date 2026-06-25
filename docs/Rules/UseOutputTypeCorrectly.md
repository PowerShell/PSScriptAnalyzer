---
description: Use the OutputType attribute correctly
ms.date: 06/11/2026
ms.topic: reference
title: UseOutputTypeCorrectly
---
# UseOutputTypeCorrectly

**Severity Level: Information**

## Description

This rule detects when a function or script returns a different type than what is declared in the
`OutputType` attribute. A function should return values that match the types specified in its
`OutputType` attribute.

The `OutputType` attribute documents what type of output a function produces, and the actual return
values should comply with this declaration. To learn more, see
[about_Functions_OutputTypeAttribute][01].

## Example

### Noncompliant

```powershell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )
        return 4
}
```

### Compliant

```powershell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )

        return 'four'
}
```

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_functions_outputtypeattribute
