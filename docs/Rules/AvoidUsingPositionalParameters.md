---
description: Avoid Using Positional Parameters
ms.custom: PSSA v1.21.0
ms.date: 06/28/2023
ms.topic: reference
title: AvoidUsingPositionalParameters
---
# AvoidUsingPositionalParameters

** Severity Level: Information **

## Description

Using positional parameters reduces the readability of code and can introduce errors. It is possible
that a future version of the cmdlet could change in a way that would break existing scripts if calls
to the cmdlet rely on the position of the parameters.

For simple cmdlets with only a few positional parameters, the risk is much smaller. To prevent this
rule from being too noisy, this rule gets only triggered when there are 3 or more parameters
supplied. A simple example where the risk of using positional parameters is negligible, is
`Test-Path $Path`.

## Configuration

```powershell
Rules = @{
    PSAvoidUsingPositionalParameters = @{
        CommandAllowList = 'az', 'Join-Path'
        Enable           = $true
    }
}
```

### Parameters

#### CommandAllowList: string[] (Default value is 'az')

Commands to be excluded from this rule. `az` is excluded by default because starting with version 2.40.0 the entrypoint of the AZ CLI became an `az.ps1` script but this script does not have any named parameters and just passes them on using `$args` as is to the Python process that it starts, therefore it is still a CLI and not a PowerShell command.

#### Enable: bool (Default value is `$true`)

Enable or disable the rule during ScriptAnalyzer invocation.

## How

Use full parameter names when calling commands.

## Example

### Wrong

```powershell
Get-Command ChildItem Microsoft.PowerShell.Management
```

### Correct

```powershell
Get-Command -Noun ChildItem -Module Microsoft.PowerShell.Management
```
