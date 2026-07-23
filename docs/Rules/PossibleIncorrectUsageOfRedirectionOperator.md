---
description: Use -gt or -ge comparison operators instead of redirection operators
ms.date: 07/23/2026
ms.topic: reference
title: PossibleIncorrectUsageOfRedirectionOperator
---
# PossibleIncorrectUsageOfRedirectionOperator

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
