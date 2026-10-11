---
description: Missing Try Block
ms.date: 07/21/2026
ms.topic: reference
title: MissingTryBlock
---
# MissingTryBlock

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule identifies instances where `catch` or `finally` blocks are present without an associated
`try` block. Without a `try` block, the `catch` and `finally` are interpreted as commands and result
in a runtime error, such as:

> "The term 'catch' is not recognized as a name of a cmdlet"

To prevent this error, add a `try` block before the `catch` and `finally` blocks.

> [!NOTE]
> This rule is triggered by functions named `catch` or `finally`. However, creating functions with
> those names violates the [AvoidReservedWordsAsFunctionNames][01] rule. If you have functions named
> `catch` or `finally`, you can either rename the function or disable this rule.

## Example

### Noncompliant

```powershell
catch { "An error occurred." }
```

### Compliant

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

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSMissingTryBlock', '')]
```

To suppress the rule for only one of the two keywords, pass `catch` or `finally` as the second
argument. The value isn't case sensitive.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSMissingTryBlock', 'finally')]
    param()

    finally { 'Cleaning up.' }
}
```

<!-- link references -->
[01]: AvoidReservedWordsAsFunctionNames.md
