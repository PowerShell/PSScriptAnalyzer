---
description: Misleading backtick
ms.date: 06/04/2026
ms.topic: reference
title: MisleadingBacktick
---
# MisleadingBacktick

**Severity Level: Warning**

## Description

This rule detects lines where a trailing backtick is followed by one or more whitespace characters.
A trailing backtick is used for line continuation only when it's the last character on the line. If
whitespace follows the backtick on the same line the continuation doesn't work as intended, which
can make the code look valid but behave unexpectedly.

## Example

### Noncompliant

```powershell
# The line in this example ends with a backtick and trailing whitespace
Get-Process `
| Where-Object CPU -gt 100
```

### Compliant

```powershell
Get-Process `
| Where-Object CPU -gt 100
```
