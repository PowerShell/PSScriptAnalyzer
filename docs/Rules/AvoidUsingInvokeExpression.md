---
description: Avoid using Invoke-Expression
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingInvokeExpression
---
# AvoidUsingInvokeExpression

**Severity Level: Warning**

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
