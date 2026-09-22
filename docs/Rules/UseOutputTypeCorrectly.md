---
description: Use the OutputType attribute correctly
ms.date: 07/21/2026
ms.topic: reference
title: UseOutputTypeCorrectly
---
# UseOutputTypeCorrectly

**Severity Level: Information**

**Default state: Always enabled**

## Description

This rule detects when a function or script returns a different type than what is declared in the
`OutputType` attribute. A function should return values that match the types specified in its
`OutputType` attribute.

The `OutputType` attribute documents what type of output a function produces, and the actual return
values should comply with this declaration. To learn more, see
[about_Functions_OutputTypeAttribute][01].

## Example

### Noncompliant

```powershell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )
        return 4
}
```

### Compliant

```powershell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )

        return 'four'
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_functions_outputtypeattribute
[02]: ../using-scriptanalyzer.md
