---
description: Missing Try Block
ms.date: 04/22/2026
ms.topic: reference
title: MissingTryBlock
---
# MissingTryBlock

**Severity Level: Warning**

## Description

The `catch` and `finally` blocks must be preceded by a `try` block. Without a `try` block, the
`catch` and `finally` are interpreted as commands and result in a runtime error, such as:

> "The term 'catch' is not recognized as a name of a cmdlet"

This rule identifies instances where `catch` or `finally` blocks are present with out an associated
`try` block.

> [!NOTE]
> This rule is not enabled by default. The user needs to enable it through settings.

## How

Add a `try` block before the `catch` and `finally` blocks.

> [!NOTE]
> This rule could result in a false positive as it will fire on user code that violates the rule
> [AvoidReservedWordsAsFunctionNames][1] for functions named `catch` or `finally`:
> If you have functions named `catch` or `finally`, you can either rename the function or disable
> this rule.

## Example

### Wrong

```powershell
catch { "An error occurred." }
```

### Correct

```powershell
try { $a = 1 / $b }
catch { "Attempted to divide by zero." }
```

## Configuration

```powershell
Rules = @{
    PSMissingTryBlock = @{
        Enable = $true
    }
}
```

### Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.

[1]: AvoidReservedWordsAsFunctionNames.md "Avoid using reserved words as function names."