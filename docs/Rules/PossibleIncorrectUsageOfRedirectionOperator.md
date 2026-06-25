---
description: Use -gt or -ge comparison operators instead of redirection operators
ms.date: 06/05/2026
ms.topic: reference
title: PossibleIncorrectUsageOfRedirectionOperator
---
# PossibleIncorrectUsageOfRedirectionOperator

**Severity Level: Information**

## Description

This rule detects the use of the sequences `>` and `>=` in conditional statements where comparison
operators are intended. This rule catches instances where `>` or `>=` appears inside `if`, `elseif`,
`while`, and `do-while` statements, which are almost always unintentional.

In many programming languages, `>` is the comparison operator for _greater than_, but PowerShell
uses `-gt` (greater than) and `-ge` (greater or equal) instead. It's easy to accidentally use the
wrong operator, especially if you're familiar with other languages.

## Example

### Noncompliant

```powershell
if ($a > $b)
{
    ...
}
```

### Compliant

```powershell
if ($a -gt $b)
{
    ...
}
```
