---
description: Use the '$using:' scope modifier in runspace scriptblocks
ms.date: 07/21/2026
ms.topic: reference
title: UseUsingScopeModifierInNewRunspaces
---
# UseUsingScopeModifierInNewRunspaces

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
