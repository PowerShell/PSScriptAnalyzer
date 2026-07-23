---
description: Changing automatic variables might have undesired side effects
ms.date: 07/21/2026
ms.topic: reference
title: AvoidAssignmentToAutomaticVariable
---
# AvoidAssignmentToAutomaticVariable

**Severity Level: Warning**

**Default state: Always enabled**

## Description

Avoid using automatic variable names in your functions and parameters. This rule detects assignments
to automatic variables and parameter names that use automatic variable names. PowerShell
automatically defines variables that store internal state information and manages them on its own.
Even though you _can_ override many automatic variables, doing so can have unexpected effects for
users and make your code harder to maintain and debug.

Reserve automatic variables for PowerShell's internal use only, and rely on them only to read state
information.

To learn more, see [about_Automatic_Variables][01].

## Example

### Noncompliant

The variable `$Error` is an automatic variable that exists in the global scope and should therefore
never be used as a variable or parameter name.

```powershell
function foo($Error){ }
```

```powershell
function Get-CustomErrorMessage($ErrorMessage){ $Error = "Error occurred: $ErrorMessage" }
```

### Compliant

```powershell
function Get-CustomErrorMessage($ErrorMessage){ $FinalErrorMessage = "Error occurred: $ErrorMessage" }
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_automatic_variables
[02]: ../using-scriptanalyzer.md
