---
description: Avoid using hardcoded computer names
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingComputerNameHardcoded
---
# AvoidUsingComputerNameHardcoded

**Severity Level: Error**

**Default state: Always enabled**

## Description

This rule detects hard-coded computer names in the `ComputerName` parameter of cmdlets. Hard-coded
computer names can expose sensitive information and reduce script portability. The `ComputerName`
parameter should always be parameterized or dynamically assigned to ensure scripts are flexible and
secure.

Use parameters, environment variables, or dynamic values instead of hard-coded computer
names.

## Example

### Noncompliant

```powershell
Function Invoke-MyRemoteCommand ()
{
    Invoke-Command -Port 343 -ComputerName HardcodedRemoteHostname
}
```

### Compliant

```powershell
Function Invoke-MyCommand ($ComputerName)
{
    Invoke-Command -Port 343 -ComputerName $ComputerName
}
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