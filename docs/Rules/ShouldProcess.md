---
description: Should Process
ms.custom: PSSA v1.20.0
ms.date: 03/24/2022
ms.topic: reference
title: ShouldProcess
---
# ShouldProcess

**Severity Level: Warning**

## Description

If a cmdlet declares the `SupportsShouldProcess` attribute, then it should also call
`ShouldProcess`. A violation is any function which either declares `SupportsShouldProcess` attribute
but makes no calls to `ShouldProcess` or it calls `ShouldProcess` but does not declare
`SupportsShouldProcess`

For more information, see the following articles:

- [about_Functions_Advanced_Methods](/powershell/modules/microsoft.powershell.core/about/about_Functions_Advanced_Methods)
- [about_Functions_CmdletBindingAttribute](/powershell/modules/microsoft.powershell.core/about/about_Functions_CmdletBindingAttribute)
- [Everything you wanted to know about ShouldProcess](/powershell/scripting/learn/deep-dives/everything-about-shouldprocess)

## How

To fix a violation of this rule, please call `ShouldProcess` method when a cmdlet declares
`SupportsShouldProcess` attribute. Or please add `SupportsShouldProcess` attribute argument when
calling `ShouldProcess`

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
        $Path,

        [Parameter(Mandatory=$true)]
        [string]$Content
    )

    if ($PSCmdlet.ShouldProcess($Path, ("Setting content to '{0}'" -f $Content)))
    {
        $Content | Out-File -FilePath $Path
    }
    else
    {
        # Code that should be processed if doing a WhatIf operation
        # Must NOT change anything outside of the function / script
    }
}
```
