---
description: Use SupportsShouldProcess
ms.date: 07/21/2026
ms.topic: reference
title: UseSupportsShouldProcess
---
# UseSupportsShouldProcess

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects manual declarations of `WhatIf` and `Confirm` parameters in functions and cmdlets.
Rather than manually declaring these parameters, you should use the `CmdletBinding` attribute with
`SupportsShouldProcess` as a named argument.

This approach automatically provides both parameters along with built-in functionality that gives
you and your users a consistent interactive experience. Using `SupportsShouldProcess` is more
maintainable and provides standard behavior without extra code.

## Example

### Noncompliant

```powershell
function foo {
    param(
        $param1,
        $Confirm,
        $WhatIf
    )
}
```

### Compliant

```powershell
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1
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
[02]: ../using-scriptanalyzer.md
