---
description: Use cmdlets correctly
ms.date: 07/21/2026
ms.topic: reference
title: UseCmdletCorrectly
---
# UseCmdletCorrectly

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects commands that are invoked without their required mandatory parameters. When you
call a command, it must be invoked with the correct syntax and all mandatory parameters specified.
Missing required parameters can cause commands to fail or behave unexpectedly.

## Example

### Noncompliant

```powershell
Function Set-TodaysDate ()
{
    Set-Date
    ...
}
```

### Compliant

```powershell
Function Set-TodaysDate ()
{
    $date = Get-Date
    Set-Date -Date $date
    ...
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
[01]: /powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands
[02]: ../using-scriptanalyzer.md