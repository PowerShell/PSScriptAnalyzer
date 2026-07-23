---
description: Avoid using Invoke-Expression
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingInvokeExpression
---
# AvoidUsingInvokeExpression

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects the use of the `Invoke-Expression` command, which poses security risks in your
scripts and applications. You must be careful when using the `Invoke-Expression` command. It
executes the specified string and returns the results.

Code injection vulnerabilities can occur if the expression passed as a string includes user-provided
data, making your application susceptible to malicious attacks. Avoid using `Invoke-Expression`
whenever possible.

## Example

### Noncompliant

```powershell
Invoke-Expression 'Get-Process'
```

### Compliant

```powershell
Get-Process
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[02]: ../using-scriptanalyzer.md