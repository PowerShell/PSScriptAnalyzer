---
description: Avoid Invoking Empty Members
ms.custom: PSSA v1.21.0
ms.date: 10/18/2021
ms.topic: reference
title: AvoidInvokingEmptyMembers
---
# AvoidInvokingEmptyMembers

**Severity Level: Warning**

## Description

Invoking non-constant members can cause potential bugs. Please double check the syntax to make sure
that invoked members are constants.

## How

Provide the requested members for a given type or class.

## Example

### Wrong

```powershell
$MyString = "abc"
$MyString.('len'+'gth')
```

### Correct

```powershell
$MyString = "abc"
$MyString.('length')
```
