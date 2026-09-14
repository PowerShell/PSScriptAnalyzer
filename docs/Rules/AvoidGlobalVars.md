---
description: Avoid global variables.
ms.date: 07/21/2026
ms.topic: reference
title: AvoidGlobalVars
---
# AvoidGlobalVars

**Severity Level: Warning**

**Default state: Always enabled**

## Description

You should avoid modifying global variables in your scripts and functions because other scripts or
functions that run in the same session can depend on them. This can lead to unexpected behavior and
make it difficult to debug your code.

This rule detects usage of variables with the global scope modifier. PowerShell controls access to
variables, functions, aliases, and drives through a mechanism known as scoping. Variables and
functions that are present when PowerShell starts were created in the global scope. Globally scoped
variables include:

- Automatic variables
- Preference variables

This rule doesn't detect use of the `New-Variable` cmdlet to create variables in the global scope or
the other `*-Variable` cmdlets to work with variables in the global scope. It only detects variable
expressions with the global scope modifier, like `$global:example`.

This rule doesn't apply to variable expressions that use the global scope modifier where the
variable name is for an [automatic variable][01] or a [preference variable][02].

Use local or script scope for variables instead of the global scope. To learn more, see
[about_Scopes][03].

## Example

### Noncompliant

```powershell
$var1 = 'foo'
function Test-NotGlobal ($var) {
    $Global:var1 = $var
}
Test-NotGlobal 'bar'
$var1
```

### Compliant

```powershell
$var1 = 'foo'
function Test-NotGlobal ($var) {
    $var1 = $var
}
Test-NotGlobal 'bar'
$var1
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][04].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_automatic_variables
[02]: /powershell/module/microsoft.powershell.core/about/about_preference_variables
[03]: /powershell/module/microsoft.powershell.core/about/about_scopes
[04]: ../using-scriptanalyzer.md
