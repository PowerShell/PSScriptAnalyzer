---
description: Equal sign is not an assignment operator. Did you mean the equality operator \'-eq\'?
ms.date: 06/28/2023
ms.topic: reference
title: PossibleIncorrectUsageOfAssignmentOperator
---
# PossibleIncorrectUsageOfAssignmentOperator

**Severity Level: Information**

## Description

In many programming languages, the equality operator is denoted as `==` or `=`, but `PowerShell`
uses `-eq`. Therefore, it can easily happen that the wrong operator is used unintentionally. This
rule catches a few special cases where the likelihood of that is quite high.

The rule looks for usages of `==` and `=` operators inside `if`, `else if`, `while` and `do-while`
statements but it does not warn if any kind of command or expression is used at the right hand side
as this is probably by design.

## Example

### Wrong

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

### Correct

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

## Implicit suppression using Clang style

There are some rare cases where assignment of variable inside an `if` statement is by design.
Instead of suppressing the rule, one can also signal that assignment was intentional by wrapping the
expression in extra parenthesis. An exception for this is when `$null` is used on the LHS because
there is no use case for this.

```powershell
if (($shortVariableName = $SuperLongVariableName['SpecialItem']['AnotherItem']))
{
    ...
}
```
