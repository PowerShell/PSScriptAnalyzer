---
description: Use SupportsShouldProcess
ms.date: 06/11/2026
ms.topic: reference
title: UseSupportsShouldProcess
---
# UseSupportsShouldProcess

**Severity Level: Warning**

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
