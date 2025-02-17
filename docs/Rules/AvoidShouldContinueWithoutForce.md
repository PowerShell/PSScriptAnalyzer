---
description: Avoid Using ShouldContinue Without Boolean Force Parameter
ms.date: 06/28/2023
ms.topic: reference
title: AvoidShouldContinueWithoutForce
---
# AvoidShouldContinueWithoutForce

**Severity Level: Warning**

## Description

Functions that use ShouldContinue should have a boolean force parameter to allow user to bypass it.

You can get more details by running `Get-Help about_Functions_CmdletBindingAttribute` and
`Get-Help about_Functions_Advanced_Methods` command in PowerShell.

## How

Call the `ShouldContinue` method in advanced functions when `ShouldProcess` method returns `$true`.

## Example

### Wrong

```powershell
Function Test-ShouldContinue
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param
    (
        $MyString = 'blah'
    )

    if ($PsCmdlet.ShouldContinue('ShouldContinue Query', 'ShouldContinue Caption'))
    {
        ...
    }
}
```

### Correct

```powershell
Function Test-ShouldContinue
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    Param
    (
        $MyString = 'blah',
        [Switch]$Force
    )

    if ($Force -or $PsCmdlet.ShouldContinue('ShouldContinue Query', 'ShouldContinue Caption'))
    {
        ...
    }
}
```
