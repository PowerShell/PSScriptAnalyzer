---
description: Use cmdlets correctly
ms.date: 06/08/2026
ms.topic: reference
title: UseCmdletCorrectly
---
# UseCmdletCorrectly

**Severity Level: Warning**

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
