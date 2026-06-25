---
description: Avoid global aliases.
ms.date: 05/28/2026
ms.topic: reference
title: AvoidGlobalAliases
---
# AvoidGlobalAliases

**Severity Level: Warning**

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

<!-- link references -->

[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_scopes
