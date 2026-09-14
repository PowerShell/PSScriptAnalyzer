---
description: Avoid using null or empty HelpMessage parameter attribute
ms.date: 07/21/2026
ms.topic: reference
title: AvoidNullOrEmptyHelpMessageAttribute
---
# AvoidNullOrEmptyHelpMessageAttribute

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects [HelpMessage][01] attributes that contain null values, empty strings, or no value
assignment. The `HelpMessage` attribute must contain a meaningful, non-empty string value. Omitting
the value altogether causes a parse-time syntax error.

Using an empty string or null value causes PowerShell to raise an error at runtime. Always provide a
descriptive help message that explains the parameter's purpose and expected input to users.

## Example

### Noncompliant

```powershell
Function BadFuncEmptyHelpMessageEmpty
{
    Param(
        [Parameter(HelpMessage='')]
        [String]
        $Param
    )

    $Param
}

Function BadFuncEmptyHelpMessageNull
{
    Param(
        [Parameter(HelpMessage=$null)]
        [String]
        $Param
    )

    $Param
}

Function BadFuncEmptyHelpMessageNoAssignment
{
    Param(
        [Parameter(HelpMessage)]
        [String]
        $Param
    )

    $Param
}
```

### Compliant

```powershell
Function GoodFuncHelpMessage
{
    Param(
        [Parameter(HelpMessage='This is helpful')]
        [String]
        $Param
    )

    $Param
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
[01]: /powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters#helpmessage-argument
[02]: ../using-scriptanalyzer.md
