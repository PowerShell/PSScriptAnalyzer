---
description: Avoid using positional parameters
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingPositionalParameters
---
# AvoidUsingPositionalParameters

**Severity Level: Information**

**Default state: Enabled**

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
Join-Path 'C:\' 'Windows' 'System32'
```

### Compliant

```powershell
Join-Path -Path 'C:\' -ChildPath 'Windows' -AdditionalChildPath 'System32'
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

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingPositionalParameters', '')]
```

To suppress the rule for a single command, pass the command name as it appears in the call as the
second argument. The name isn't case sensitive, but it must match what's written in the script, so
a call made through an alias needs the alias name. To exclude a command everywhere, use the
`CommandAllowList` setting instead.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingPositionalParameters', 'Set-ItemProperty')]
    param()

    Set-ItemProperty foo.txt IsReadOnly $true
}
```
