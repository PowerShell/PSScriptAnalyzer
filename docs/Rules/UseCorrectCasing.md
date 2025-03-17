---
description: Use exact casing of cmdlet/function/parameter name.
ms.date: 06/28/2023
ms.topic: reference
title: UseCorrectCasing
---
# UseCorrectCasing

**Severity Level: Information**

## Description

This is a style formatting rule. PowerShell is case insensitive where applicable. The casing of
cmdlet names or parameters does not matter but this rule ensures that the casing matches for
consistency and also because most cmdlets/parameters start with an upper case and using that
improves readability to the human eye.

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

### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

### CheckCommands: bool (Default value is `$true`)

If true, require the case of all operators to be lowercase.

### CheckKeyword: bool (Default value is `$true`)

If true, require the case of all keywords to be lowercase.

### CheckOperator: bool (Default value is `$true`)

If true, require the case of all commands to match their actual casing.

## How

Use exact casing of the cmdlet and its parameters, e.g.
`Invoke-Command { 'foo' } -RunAsAdministrator`.

## Example

### Wrong

```powershell
invoke-command { 'foo' } -runasadministrator
```

### Correct

```powershell
Invoke-Command { 'foo' } -RunAsAdministrator
```
