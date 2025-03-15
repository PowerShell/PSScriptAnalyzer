---
description: Changing automatic variables might have undesired side effects
ms.date: 06/28/2023
ms.topic: reference
title: AvoidAssignmentToAutomaticVariable
---
# AvoidAssignmentToAutomaticVariable

**Severity Level: Warning**

## Description

PowerShell has built-in variables known as automatic variables. Many of them are read-only and
PowerShell throws an error when trying to assign an value on those. Other automatic variables should
only be assigned in certain special cases to achieve a certain effect as a special technique.

To understand more about automatic variables, see `Get-Help about_Automatic_Variables`.

<!-- TODO
Ability to suppress was added in https://github.com/PowerShell/PSScriptAnalyzer/pull/1896
Need documentation for how to configure suppression of this rule.
-->

## How

Use variable names in functions or their parameters that do not conflict with automatic variables.

## Example

### Wrong

The variable `$Error` is an automatic variables that exists in the global scope and should therefore
never be used as a variable or parameter name.

```powershell
function foo($Error){ }
```

```powershell
function Get-CustomErrorMessage($ErrorMessage){ $Error = "Error occurred: $ErrorMessage" }
```

### Correct

```powershell
function Get-CustomErrorMessage($ErrorMessage){ $FinalErrorMessage = "Error occurred: $ErrorMessage" }
```
