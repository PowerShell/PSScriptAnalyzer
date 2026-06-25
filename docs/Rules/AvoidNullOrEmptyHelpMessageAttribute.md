---
description: Avoid using null or empty HelpMessage parameter attribute
ms.date: 06/01/2026
ms.topic: reference
title: AvoidNullOrEmptyHelpMessageAttribute
---
# AvoidNullOrEmptyHelpMessageAttribute

**Severity Level: Warning**

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

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters#helpmessage-argument
