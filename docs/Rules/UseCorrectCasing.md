---
description: Use exact casing for cmdlet names, functions, and parameters
ms.date: 06/10/2026
ms.topic: reference
title: UseCorrectCasing
---
# UseCorrectCasing

**Severity Level: Information**

## Description

This rule detects inconsistent casing in cmdlet names, parameters, type names, keywords, and
operators. PowerShell is case-insensitive wherever possible, so the casing of cmdlet names,
parameters, keywords, and operators don't affect functionality.

However, this rule ensures consistent casing for clarity and readability. Using lowercase keywords
helps distinguish them from commands, while using lowercase operators helps distinguish them from
parameters.

To follow this rule:

- Use exact casing for type names.
- Use exact casing for cmdlet and parameter names.
- Use lowercase for language keywords and operators.

## Example

### Noncompliant

```powershell
ForEach ($file in Get-childitem -Recurse) {
    $file.Extension -EQ '.txt'
}

invoke-command { 'foo' } -runasadministrator
```

### Compliant

```powershell
foreach ($file in Get-ChildItem -Recurse) {
    $file.Extension -eq '.txt'
}

Invoke-Command { 'foo' } -RunAsAdministrator
```

## Configuration

```powershell
Rules = @{
    PSUseCorrectCasing = @{
        Enable        = $true
        CheckCommands = $true
        CheckKeyword  = $true
        CheckOperator = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks code against this rule. It accepts a boolean
value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### CheckCommands

This parameter controls whether ScriptAnalyzer checks that the casing of all command and parameter
names matches their canonical casing. It accepts a boolean value. The default value is `$true`.

### CheckKeyword

This parameter controls whether ScriptAnalyzer checks that all language keywords are lowercase. It
accepts a boolean value. The default value is `$true`.

### CheckOperator

This parameter controls whether ScriptAnalyzer checks that all operators are lowercase. For example,
`-eq`, `-ne`, and `-gt`. It accepts a boolean value. The default value is `$true`.
