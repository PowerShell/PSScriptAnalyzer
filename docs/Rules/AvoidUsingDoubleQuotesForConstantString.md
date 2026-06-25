---
description: Avoid using double quotes if the string is constant
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingDoubleQuotesForConstantString
---
# AvoidUsingDoubleQuotesForConstantString

**Severity Level: Information**

## Description

This rule detects static strings enclosed with double quotes (`"<text>"`) with text that doesn't
contain variables, expressions, or special characters that require escaping.

Enclose text with single quotes (`'<text>'`) when the value of a string is constant. A constant
string doesn't contain variables or expressions intended to insert values into the string, such as
`"$PID-$(hostname)"`. Using single quotes makes the intent clearer that the string is constant and
allows you to use special characters such as `$` without needing to escape them.

However, there are exceptions where this rule doesn't flag violations:

- Double-quoted strings are preferred when the string contains single quotes
- Embedded escape sequences like newline (``"`n"``)
- Other special characters that require escaping

## Example

### Noncompliant

```powershell
$constantValue = "I Love PowerShell"
```

### Compliant

```powershell
$constantValue = 'I Love PowerShell'
```
