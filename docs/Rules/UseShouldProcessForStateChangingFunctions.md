---
description: Use ShouldProcess For State Changing Functions
ms.date: 12/05/2024
ms.topic: reference
title: UseShouldProcessForStateChangingFunctions
---
# UseShouldProcessForStateChangingFunctions

**Severity Level: Warning**

## Description

Functions whose verbs change system state should support `ShouldProcess`. To enable the
`ShouldProcess` feature, set the `SupportsShouldProcess` argument in the `CmdletBinding` attribute.
The `SupportsShouldProcess` argument adds **Confirm** and **WhatIf** parameters to the function. The
**Confirm** parameter prompts the user before it runs the command on each object in the pipeline.
The **WhatIf** parameter lists the changes that the command would make, instead of running the
command.

Verbs that should support `ShouldProcess`:

- `New`
- `Set`
- `Remove`
- `Start`
- `Stop`
- `Restart`
- `Reset`
- `Update`

## How

Include the `SupportsShouldProcess` argument in the `CmdletBinding` attribute.

## Example

### Wrong

```powershell
function Set-ServiceObject
{
    [CmdletBinding()]
    param
    (
        [string]
        $Parameter1
    )
    ...
}
```

### Correct

```powershell
function Set-ServiceObject
{
    [CmdletBinding(SupportsShouldProcess = $true)]
    param
    (
        [string]
        $Parameter1
    )
    ...
}
```

## More information

- [about_Functions_CmdletBindingAttribute][01]
- [Everything you wanted to know about ShouldProcess][04]
- [Required Development Guidelines][03]
- [Requesting Confirmation from Cmdlets][02]

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_functions_cmdletbindingattribute
[02]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/requesting-confirmation-from-cmdlets
[03]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/required-development-guidelines#support-confirmation-requests-rd04
[04]: https://learn.microsoft.com/powershell/scripting/learn/deep-dives/everything-about-shouldprocess
