---
description: Create hashtables with literal initializers
ms.date: 06/11/2026
ms.topic: reference
title: UseLiteralInitializerForHashtable
---
# UseLiteralInitializerForHashtable

**Severity Level: Warning**

## Description

This rule detects hashtables created using the `[hashtable]::new()` method or the `New-Object
-TypeName hashtable` cmdlet without passing an `IEqualityComparer` object. When you create a
hashtable using `[hashtable]::new()` or `New-Object -TypeName hashtable`, the keys are looked up in
a case-sensitive manner by default.

However, PowerShell is case-insensitive in nature, and hashtables should maintain this behavior. To
ensure consistent case-insensitive key lookup, use literal hashtable expressions instead.

## Example

### Noncompliant

```powershell
$hashtable = [hashtable]::new()
```

### Compliant

```powershell
$hashtable = @{}
```
