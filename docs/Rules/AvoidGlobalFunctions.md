---
description: Avoid global functions and aliases
ms.date: 06/28/2023
ms.topic: reference
title: AvoidGlobalFunctions
---
# AvoidGlobalFunctions

**Severity Level: Warning**

## Description

This rule detects function definitions that use the `global:` scope modifier on the function name to
define a function in the global scope. Global functions can unintentionally override existing
functions in the session, leading to unexpected behavior and name collisions. Name collisions make
it difficult for module consumers to diagnose issues and maintain code reliability.

To avoid this issue, define functions without the global scope modifier, or use other appropriate
scope modifiers. To learn more, see [about_Scopes][01].

## Example

### Noncompliant

```powershell
function global:functionName {}
```

### Compliant

```powershell
function functionName {}
```

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_scopes
