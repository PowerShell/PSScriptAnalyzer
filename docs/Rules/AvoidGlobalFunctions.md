---
description: Avoid global functions and aliases
ms.custom: PSSA v1.21.0
ms.date: 10/18/2021
ms.topic: reference
title: AvoidGlobalFunctions
---
# AvoidGlobalFunctions

**Severity Level: Warning**

## Description

Globally scoped functions override existing functions within the sessions with matching names. This
name collision can cause difficult to debug issues for consumers of modules.


To understand more about scoping, see `Get-Help about_Scopes`.

## How

Use other scope modifiers for functions.

## Example

### Wrong

```powershell
function global:functionName {}
```

### Correct

```powershell
function functionName {}
```
