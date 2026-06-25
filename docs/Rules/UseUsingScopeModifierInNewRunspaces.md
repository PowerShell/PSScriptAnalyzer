---
description: Use the '$using:' scope modifier in runspace scriptblocks
ms.date: 06/11/2026
ms.topic: reference
title: UseUsingScopeModifierInNewRunspaces
---
# UseUsingScopeModifierInNewRunspaces

**Severity Level: Warning**

## Description

This rule detects when scriptblocks running in new runspaces reference parent scope variables
without the `$using:` scope modifier. When a scriptblock is intended to run in a new runspace, it
can't directly access variables from the parent scope.

You must either use the `$using:` scope modifier to explicitly reference a parent scope variable, or
initialize the variable within the scriptblock itself. This rule applies to:

- `Invoke-Command`- Only with the **ComputerName** or **Session** parameter.
- `Workflow { InlineScript {} }` (supported only in Windows PowerShell 5.1 and earlier; not
  available in PowerShell 6 or higher)
- `Foreach-Object` - Only with the **Parallel** parameter
- `Start-Job`
- `Start-ThreadJob`
- The `Script` resource in Desired State Configuration (DSC) configurations, specifically for the
  `GetScript`, `SetScript`, and `TestScript` properties.

## Example

### Noncompliant

```powershell
$var = 'foo'
1..2 | ForEach-Object -Parallel { $var }
```

### Compliant

```powershell
$var = 'foo'
1..2 | ForEach-Object -Parallel { $using:var }
```
