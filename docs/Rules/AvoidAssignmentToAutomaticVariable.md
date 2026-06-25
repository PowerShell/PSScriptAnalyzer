---
description: Changing automatic variables might have undesired side effects
ms.date: 06/12/2026
ms.topic: reference
title: AvoidAssignmentToAutomaticVariable
---
# AvoidAssignmentToAutomaticVariable

**Severity Level: Warning**

## Description

This rule detects assignments to automatic variables and parameter names that use automatic variable
names. PowerShell automatically defines variables that store internal state information and manages
them on its own. Even though you _can_ override many automatic variables, doing so can have
unexpected effects for users and make your code harder to maintain and debug.

Avoid using automatic variable names in your functions and parameters. Reserve automatic variables
for PowerShell's internal use only, and rely on them only to read state information.

To learn more, see [about_Automatic_Variables][01].

<!-- TODO
Ability to suppress was added in https://github.com/PowerShell/PSScriptAnalyzer/pull/1896
Need documentation for how to configure suppression of this rule.
-->

## How

Use variable names in functions or their parameters that do not conflict with automatic variables.

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

<!-- Link reference definitions -->

[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_automatic_variables
