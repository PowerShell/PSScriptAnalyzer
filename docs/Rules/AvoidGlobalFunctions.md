---
description: Avoid global functions and aliases
ms.date: 07/21/2026
ms.topic: reference
title: AvoidGlobalFunctions
---
# AvoidGlobalFunctions

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_scopes
[02]: ../using-scriptanalyzer.md
