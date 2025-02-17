---
description: Create hashtables with literal initializers
ms.date: 06/28/2023
ms.topic: reference
title: UseLiteralInitializerForHashtable
---
# UseLiteralInitializerForHashtable

**Severity Level: Warning**

## Description

Creating a hashtable using `[hashtable]::new()` or `New-Object -TypeName hashtable` without passing
a `IEqualityComparer` object to the constructor creates a hashtable where the keys are looked-up in
a case-sensitive manner. However, PowerShell is case-insensitive in nature and it is best to create
hashtables with case-insensitive key look-up.

This rule is intended to warn the author of the case-sensitive nature of the hashtable when created
using the `new` method or the `New-Object` cmdlet.

## How to Fix

Create the hashtable using a literal hashtable expression.

## Example

### Wrong

```powershell
$hashtable = [hashtable]::new()
```

### Wrong

```powershell
$hashtable = New-Object -TypeName hashtable
```

### Correct

```powershell
$hashtable = @{}
```
