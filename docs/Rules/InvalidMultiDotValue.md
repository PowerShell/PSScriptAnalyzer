---
description: Invalid unquoted multi-dot value construction
ms.date: 04/24/2024
ms.topic: reference
title: InvalidMultiDotValue
---
# InvalidMultiDotValue

**Severity Level: Error**

## Description

PowerShell doesn't support unquoted literal values with multiple dots (`.`). Any value with two or
more dots results in `$null`. This rule identifies instances where such values are used, which can
lead to unexpected behavior or errors in the code.

To create values of the intended type, enclose the value in quotes and use type-casting or use type
constructor methods to create the appropriate object.


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

## Configuration

```powershell
Rules = @{
    PSAvoidExclaimOperator  = @{
        Enable = $true
    }
}
```

### Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.
