---
description: Missing Try Block
ms.date: 04/22/2026
ms.topic: reference
title: MissingTryBlock
---
# MissingTryBlock

**Severity Level: Error**

## Description

The `catch` and `finally` blocks should be preceded by a `try` block.
Otherwise, the `catch` and `finally` blocks will be interpreted as commands, which is likely a mistake and result
in a "*The term 'catch' is not recognized as a name of a cmdlet*" error at runtime.

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
