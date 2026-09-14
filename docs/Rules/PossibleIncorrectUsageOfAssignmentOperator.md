---
description: Use the PowerShell equality operator (-eq) instead of assignment (=) in conditional statements
ms.date: 07/21/2026
ms.topic: reference
title: PossibleIncorrectUsageOfAssignmentOperator
---
# PossibleIncorrectUsageOfAssignmentOperator

**Severity Level: Information**

**Default state: Always enabled**

## Description

This rule detects when conditional statements use `=` or `==` instead of the PowerShell equality
operator (`-eq`). The rule looks for usages of `==` and `=` operators inside `if`, `elseif`,
`while`, and `do-while` statements. In PowerShell, `=` represents the [assignment operator][01] and
`-eq` represents the [equality comparison operator][02].

In many programming languages, `==` represents the equality comparison operator. When you define
`==` in a PowerShell expression, including conditional statements, PowerShell raises an error
because `==` isn't valid PowerShell syntax. This rule _always_ flags uses of `==` in conditional
statements. However, it doesn't flag uses of `=` when the right hand side of the conditional uses
any commands or expressions.

In these cases, it's likely that the use of the assignment operator is intentional. This
construction is often used to concisely assign a value to the variable or property on the left hand
side _and_ check whether the assigned value evaluates as true.

### Implicit suppression using Clang style

There are some rare cases where assigning a variable inside an `if` statement is intentional.
Instead of suppressing the rule, you can signal that the assignment's intentional by wrapping the
expression in extra parentheses. An exception applies when `$null` is used on the left-hand side
because there's no use case for it. For example:

```powershell
if (($shortVariableName = $SuperLongVariableName['SpecialItem']['AnotherItem'])) {
    ...
}
```

## Example

### Noncompliant

```powershell
if ($a = $b)
{
    ...
}
```

```powershell
if ($a == $b)
{

}
```

### Compliant

```powershell
if ($a -eq $b) # Compare $a with $b
{
    ...
}
```

```powershell
if ($a = Get-Something) # Only execute action if command returns something and assign result to variable
{
    Do-SomethingWith $a
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][03].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_assignment_operators
[02]: /powershell/module/microsoft.powershell.core/about/about_comparison_operators#-eq-and--ne
[03]: ../using-scriptanalyzer.md