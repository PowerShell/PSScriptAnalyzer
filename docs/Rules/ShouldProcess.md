---
description: Should Process
ms.custom: PSSA v1.20.0
ms.date: 01/23/2022
ms.topic: reference
title: ShouldProcess
---
# ShouldProcess

**Severity Level: Warning**

## Description

If a cmdlet declares the `SupportsShouldProcess` attribute, then it should also call
`ShouldProcess`. A violation is any function which either declares `SupportsShouldProcess` attribute
but makes no calls to `ShouldProcess` or it calls `ShouldProcess` but does not declare
`SupportsShouldProcess`.

For more information, please refer to `about_Functions_Advanced_Methods` and
`about_Functions_CmdletBindingAttribute`.

## How

To fix a violation of this rule, please call `ShouldProcess` method when a cmdlet declares
`SupportsShouldProcess` attribute. Or please add `SupportsShouldProcess` attribute argument when
calling `ShouldProcess`.

## Example

### Wrong

```powershell
function Set-File
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param
    (
        # Path to file
        [Parameter(Mandatory=$true)]
        $Path
    )

    "String" | Out-File -FilePath $Path
}
```

### Correct

```powershell
function Set-File
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param
    (
        # Path to file
        [Parameter(Mandatory=$true)]
        $Path
    )

    if ($PSCmdlet.ShouldProcess($Path, "Write"))
    {
        "String" | Out-File -FilePath $Path
    }
}
```
