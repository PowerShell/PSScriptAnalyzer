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
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][01].

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingComputerNameHardcoded', '')]
```

To suppress the rule for a single command, pass the command name as it appears in the call as the
second argument. The name isn't case sensitive, but it must match what's written in the script, so
a call made through an alias needs the alias name.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingComputerNameHardcoded', 'Invoke-Command')]
    param()

    Invoke-Command -ComputerName HardcodedHostname -Port 343
}
```

<!-- Link references -->
[01]: ../using-scriptanalyzer.md#suppressing-rules
