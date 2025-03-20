---
description: Use exact casing of cmdlet/function/parameter name.
ms.date: 03/19/2025
ms.topic: reference
title: UseCorrectCasing
---
# UseCorrectCasing

**Severity Level: Information**

## Description

This is a style/formatting rule. PowerShell is case insensitive wherever possible, so the casing of
cmdlet names, parameters, keywords and operators does not matter. This rule nonetheless ensures
consistent casing for clarity and readability. Using lowercase keywords helps distinguish them from
commands. Using lowercase operators helps distinguish them from parameters.

## How

- Use exact casing for type names.
- Use exact casing of the cmdlet and its parameters.
- Use lowercase for language keywords and operators.

## Configuration

```powershell
Rules = @{
    PS UseCorrectCasing = @{
        Enable        = $true
        CheckCommands = $true
        CheckKeyword  = $true
        CheckOperator = $true
    }
}
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### CheckCommands: bool (Default value is `$true`)

If true, require the case of all operators to be lowercase.

#### CheckKeyword: bool (Default value is `$true`)

If true, require the case of all keywords to be lowercase.

#### CheckOperator: bool (Default value is `$true`)

If true, require the case of all commands to match their actual casing.

## Examples

### Wrong way

```powershell
ForEach ($file in Get-childitem -Recurse) {
    $file.Extension -eq '.txt'
}

invoke-command { 'foo' } -runasadministrator
```

### Correct way

```powershell
foreach ($file in Get-ChildItem -Recurse) {
    $file.Extension -eq '.txt'
}

Invoke-Command { 'foo' } -RunAsAdministrator
```
