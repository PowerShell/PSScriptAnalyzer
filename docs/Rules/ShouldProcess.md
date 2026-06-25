---
description: Pair ShouldProcess with SupportsShouldProcess
ms.date: 06/08/2026
ms.topic: reference
title: ShouldProcess
---
# ShouldProcess

**Severity Level: Warning**

## Description

This rule detects mismatches between `SupportsShouldProcess` declarations and `ShouldProcess` calls.
When a cmdlet declares the `SupportsShouldProcess` attribute, it should also call the
`ShouldProcess` method.

Violations occur when:

- A function declares `SupportsShouldProcess` but doesn't call `ShouldProcess`
- A function calls `ShouldProcess` but doesn't declare `SupportsShouldProcess`

To fix this violation, ensure that `ShouldProcess` calls are paired with the `SupportsShouldProcess`
attribute declaration.

For more information, see the following articles:

- [about_Functions_Advanced_Methods][01]
- [about_Functions_CmdletBindingAttribute][02]
- [Everything you wanted to know about ShouldProcess][03]

## Example

### Noncompliant

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
    'String' | Out-File -FilePath $Path
}
```

### Compliant

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

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods
[02]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_Functions_CmdletBindingAttribute
[03]: https://learn.microsoft.com/powershell/scripting/learn/deep-dives/everything-about-shouldprocess
