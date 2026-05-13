---
description: Invalid unquoted multi-dot value construction
ms.date: 04/24/2024
ms.topic: reference
title: InvalidMultiDotValue
---
# InvalidMultiDotValue

**Severity Level: Error**

## Description

PowerShell does not support an implicit value with multiple dots.
Any *unquoted* value with 2 or more dots will not be treated as any special type (like a `version` or `IPAddress`)
but result in `$null`. These objects need to be constructed from either a quoted string (e.g. `[Version]'1.2.3'`)
or their individual components (e.g. `[Version]::new(1, 2, 3)`).


## Example

### Wrong

```powershell
$version = 1.2.3
```

or even:

```powershell
$IP = [System.Net.IPAddress]127.0.0.1
```

Where both examples will result in `$null` instead of any specific object.

### Correct

```powershell
# Use type-casting with quoted value
$IP = [System.Net.IPAddress]'127.0.0.1'
$version = [Version]'1.2.3'

# Use type constructor method
$version = [Version]::new(1, 2, 3)
```
