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

## How

Add a `try` block before the `catch` and `finally` blocks.

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
