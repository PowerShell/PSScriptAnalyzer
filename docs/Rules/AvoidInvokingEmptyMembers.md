---
description: Avoid invoking empty members.
ms.date: 05/28/2026
ms.topic: reference
title: AvoidInvokingEmptyMembers
---
# AvoidInvokingEmptyMembers

**Severity Level: Warning**

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

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_operators#member-access-operator-
