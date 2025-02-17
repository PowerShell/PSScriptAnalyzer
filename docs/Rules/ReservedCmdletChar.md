---
description: Reserved Cmdlet Chars
ms.date: 06/28/2023
ms.topic: reference
title: ReservedCmdletChar
---
# ReservedCmdletChar

**Severity Level: Error**

## Description

You cannot use following reserved characters in a function or cmdlet name as these can cause parsing
or runtime errors.

Reserved Characters include: ``#,(){}[]&/\\$^;:\"'<>|?@`*%+=~``

## How

Remove reserved characters from names.

## Example

### Wrong

```powershell
function MyFunction[1]
{...}
```

### Correct

```powershell
function MyFunction
{...}
```
