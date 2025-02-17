---
description: \'>\' is not a comparison operator. Use \'-gt\' (greater than) or \'-ge\' (greater or equal).
ms.date: 06/28/2023
ms.topic: reference
title: PossibleIncorrectUsageOfRedirectionOperator
---
# PossibleIncorrectUsageOfRedirectionOperator

**Severity Level: Information**

## Description

In many programming languages, the comparison operator for 'greater than' is `>` but `PowerShell`
uses `-gt` for it and `-ge` (greater or equal) for `>=`. Therefore, it can easily happen that the
wrong operator is used unintentionally. This rule catches a few special cases where the likelihood
of that is quite high.

The rule looks for usages of `>` or `>=` operators inside `if`, `elseif`, `while` and `do-while`
statements because this is likely going to be unintentional usage.

## Example

### Wrong

```powershell
if ($a > $b)
{
    ...
}
```

### Correct

```powershell
if ($a -gt $b)
{
    ...
}
```
