---
description: Switch parameters should not default to $true
ms.date: 06/12/2026
ms.topic: reference
title: AvoidDefaultValueSwitchParameter
---
# AvoidDefaultValueSwitchParameter

**Severity Level: Warning**

## Description

This rule detects switch parameters that are assigned a default value of `$true`. Switch parameters
shouldn't have default values. By design, a switch parameter is `$false` when not specified and
`$true` when included in the command. Assigning a default value of `$true` to a switch parameter
violates this design principle and can cause unexpected behavior.

If your parameter needs to accept only `true` and `false` values, use the `[Switch]` type instead of
`[Boolean]`. PowerShell automatically handles switch parameters correctly without requiring a
default value.

To fix this issue, remove the default value from the switch parameter declaration. The switch
naturally defaults to `$false` when not specified, allowing your logic to respond appropriately to
the caller's input.

To learn more, see [Strongly Encouraged Development Guidelines][01].

## Example

### Noncompliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Param1,

        [switch]
        $Switch=$True
    )
    ...
}
```

### Compliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Param1,

        [switch]
        $Switch
    )

}
```

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines#parameters-that-take-true-and-false
