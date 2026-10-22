---
description: Avoid Default Value For Mandatory Parameter
ms.date: 07/21/2026
ms.topic: reference
title: AvoidDefaultValueForMandatoryParameter
---
# AvoidDefaultValueForMandatoryParameter

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects when mandatory parameters have default values assigned. Mandatory parameters
shouldn't have default values because they can't be used. When a parameter is marked as mandatory,
PowerShell always prompts the user for a value if one isn't supplied when calling the function. Any
default value assigned to a mandatory parameter is unreachable and serves no purpose.

## Example

### Noncompliant

```powershell
function Test
{

    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        $Parameter1 = 'default Value'
    )
}
```

### Compliant

```powershell
function Test
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        $Parameter1
    )
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
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidDefaultValueForMandatoryParameter', '')]
```

To suppress the rule for a single parameter, pass the parameter name without the leading `$` as
the second argument. The name isn't case sensitive.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidDefaultValueForMandatoryParameter', 'Parameter')]
    param(
        [Parameter(Mandatory)]
        $Parameter = 'Default Value'
    )
}
```

<!-- Link references -->
[01]: ../using-scriptanalyzer.md#suppressing-rules
