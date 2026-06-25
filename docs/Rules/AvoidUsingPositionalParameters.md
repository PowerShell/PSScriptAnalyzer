---
description: Avoid using positional parameters
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingPositionalParameters
---
# AvoidUsingPositionalParameters

**Severity Level: Information**

## Description

This rule detects when commands are called with three or more positional parameters instead of using
named parameters. Using positional parameters reduces code readability and can introduce errors.
It's possible that a future version of the cmdlet could change in a way that'll break existing
scripts if they rely on parameter position.

For simple cmdlets with only a few positional parameters, the risk is much smaller. To prevent this
rule from being too noisy, don't supply three or more parameters. A simple example where the risk of
using positional parameters is negligible is `Test-Path $Path`.

Use full parameter names when calling commands.

## Example

### Noncompliant

```powershell
Get-Command ChildItem Microsoft.PowerShell.Management
```

### Compliant

```powershell
Get-Command -Noun ChildItem -Module Microsoft.PowerShell.Management
```

## Configure rule

```powershell
Rules = @{
    PSAvoidUsingPositionalParameters = @{
        CommandAllowList = 'Join-Path', 'MyCmdletOrScript'
        Enable           = $true
    }
}
```

## Parameters

### CommandAllowList

This parameter specifies commands or scripts to be excluded from this rule. It accepts a string
array. The default value is `@()`.

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To disable this rule, set this parameter to `$false`. The default value is `$true`.
