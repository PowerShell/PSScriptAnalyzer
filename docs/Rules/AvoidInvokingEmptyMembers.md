---
description: Avoid invoking empty members.
ms.date: 07/21/2026
ms.topic: reference
title: AvoidInvokingEmptyMembers
---
# AvoidInvokingEmptyMembers

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects expressions where the code uses the [member-access operator][01] (`.`) where the
member name is dynamically constructed. Invoking dynamically constructed member names can introduce
unexpected behavior and runtime errors. Ensure that member invocations use constant, literal member
names rather than expressions that are evaluated at runtime.

Replace dynamic member name expressions with constant member names for the target type or class.

## Example

### Noncompliant

```powershell
$MyString = 'abc'
$MyString.('len'+'gth')
```

### Compliant

```powershell
$MyString = 'abc'
$MyString.('length')
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_operators#member-access-operator-
[02]: ../using-scriptanalyzer.md
