---
description: Avoid using ShouldContinue without boolean Force parameter
ms.date: 07/21/2026
ms.topic: reference
title: AvoidShouldContinueWithoutForce
---
# AvoidShouldContinueWithoutForce

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects functions that use `ShouldContinue` without a **Force** parameter. Functions that
use `ShouldContinue` should have a boolean `Force` parameter to allow users to bypass the
confirmation prompt.

When using `ShouldContinue` in advanced functions, call it after the
`ShouldProcess` method returns `$true`.

To learn more, see [about_Functions_CmdletBindingAttribute][01] and
[about_Functions_Advanced_Methods][02].

## Example

### Noncompliant

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

### Compliant

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][03].

<!-- Link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_functions_cmdletbindingattribute
[02]: /powershell/module/microsoft.powershell.core/about/about_functions_advanced_methods
[03]: ../using-scriptanalyzer.md
