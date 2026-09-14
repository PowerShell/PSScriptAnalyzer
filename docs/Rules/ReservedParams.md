---
description: Reserved parameters
ms.date: 07/21/2026
ms.topic: reference
title: ReservedParams
---
# ReservedParams

**Severity Level: Error**

**Default state: Always enabled**

## Description

This rule detects when you attempt to redefine [common parameters][01] in an advanced function. When
you use the `CmdletBinding` or `Parameter` attributes, you're creating an advanced function. Common
parameters are automatically available in advanced functions, so you can't redefine them. If you're
trying to use a parameter name that conflicts with a common parameter, you need to change the name
of your parameter to something else.

## Example

### Noncompliant

```powershell
function Test
{
    [CmdletBinding()]
    Param
    (
        $ErrorVariable,
        $Parameter2
    )
}
```

### Compliant

```powershell
function Test
{
    [CmdletBinding()]
    Param
    (
        $Err,
        $Parameter2
    )
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
[01]: /powershell/module/microsoft.powershell.core/about/about_commonparameters
[02]: ../using-scriptanalyzer.md
