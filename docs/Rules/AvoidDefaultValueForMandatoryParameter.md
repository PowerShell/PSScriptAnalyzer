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
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[02]: ../using-scriptanalyzer.md