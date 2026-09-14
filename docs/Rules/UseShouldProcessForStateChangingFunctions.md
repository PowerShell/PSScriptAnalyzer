---
description: Use ShouldProcess for state changing functions
ms.date: 07/21/2026
ms.topic: reference
title: UseShouldProcessForStateChangingFunctions
---
# UseShouldProcessForStateChangingFunctions

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][05].

## See also

- [about_Functions_CmdletBindingAttribute][01]
- [Everything you wanted to know about ShouldProcess][04]
- [Required development guidelines][03]
- [Requesting confirmation from cmdlets][02]

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_functions_cmdletbindingattribute
[02]: /powershell/scripting/developer/cmdlet/requesting-confirmation-from-cmdlets
[03]: /powershell/scripting/developer/cmdlet/required-development-guidelines#support-confirmation-requests-rd04
[04]: /powershell/scripting/learn/deep-dives/everything-about-shouldprocess
[05]: ../using-scriptanalyzer.md
