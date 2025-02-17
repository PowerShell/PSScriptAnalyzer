---
description: Use exact casing of cmdlet/function/parameter name.
ms.date: 06/28/2023
ms.topic: reference
title: UseCorrectCasing
---
# UseCorrectCasing

**Severity Level: Information**

## Description

This is a style/formatting rule. PowerShell is case insensitive where applicable. The casing of
cmdlet names or parameters does not matter but this rule ensures that the casing matches for
consistency and also because most cmdlets/parameters start with an upper case and using that
improves readability to the human eye.

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
