---
description: Avoid global aliases.
ms.date: 07/21/2026
ms.topic: reference
title: AvoidGlobalAliases
---
# AvoidGlobalAliases

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects the use of the `New-Alias` command to create aliases in the global scope. Global
aliases can unintentionally override existing aliases in the session, leading to unexpected behavior
and name collisions. Name collisions make it difficult for module consumers to diagnose issues and
maintain code reliability.

To avoid this issue, define aliases without the **Scope** parameter, or use other appropriate scope
modifiers. To learn more, see [about_Scopes][01].

## Example

### Noncompliant

```powershell
New-Alias -Name Name -Value Value -Scope Global
```

### Compliant

```powershell
New-Alias -Name Name1 -Value Value
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
