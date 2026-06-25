---
description: Use ShouldProcess for state changing functions
ms.date: 06/11/2026
ms.topic: reference
title: UseShouldProcessForStateChangingFunctions
---
# UseShouldProcessForStateChangingFunctions

**Severity Level: Warning**

## Description

This rule detects functions with state-changing verbs that don't support `ShouldProcess`. Functions
whose verbs change system state should support `ShouldProcess` to provide users with the ability to
confirm or preview changes before execution. To enable this feature, set the `SupportsShouldProcess`
argument to `$true` in the `CmdletBinding` attribute.

The `SupportsShouldProcess` argument automatically adds the **Confirm** and **WhatIf** parameters to
your function:

- The **Confirm** parameter prompts the user to confirm the command before it runs on each object in
  the pipeline.
- The **WhatIf** parameter displays the changes the command would make without actually running the
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

## Example

### Noncompliant

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

### Compliant

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

## See also

- [about_Functions_CmdletBindingAttribute][01]
- [Everything you wanted to know about ShouldProcess][04]
- [Required development guidelines][03]
- [Requesting confirmation from cmdlets][02]

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_functions_cmdletbindingattribute
[02]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/requesting-confirmation-from-cmdlets
[03]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/required-development-guidelines#support-confirmation-requests-rd04
[04]: https://learn.microsoft.com/powershell/scripting/learn/deep-dives/everything-about-shouldprocess
