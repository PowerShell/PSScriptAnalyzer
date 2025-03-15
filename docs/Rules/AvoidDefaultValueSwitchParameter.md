---
description: Switch Parameters Should Not Default To True
ms.date: 12/05/2024
ms.topic: reference
title: AvoidDefaultValueSwitchParameter
---
# AvoidDefaultValueSwitchParameter

**Severity Level: Warning**

## Description

If your parameter takes only `true` and `false`, define the parameter as type `[Switch]`. PowerShell
treats a switch parameter as `true` when it's used with a command. If the parameter isn't included
with the command, PowerShell considers the parameter to be false. Don't define `[Boolean]`
parameters.

You shouldn't define a switch parameter with a default value of `$true` because this isn't the
expected behavior of a switch parameter.

## How

Change the default value of the switch parameter to be `$false` or don't provide a default value.
Write the logic of the script to assume that the switch parameter default value is `$false` or not
provided.

## Example

### Wrong

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

### Correct

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

    begin {
        # Ensure that the $Switch is set to false if not provided
        if (-not $PSBoundParameters.ContainsKey('Switch')) {
            $Switch = $false
        }
    }
    ...
}
```

## More information

- [Strongly Encouraged Development Guidelines][01]

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/strongly-encouraged-development-guidelines#parameters-that-take-true-and-false
