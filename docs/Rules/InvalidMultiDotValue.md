---
description: Invalid unquoted multi-dot value construction
ms.date: 07/21/2026
ms.topic: reference
title: InvalidMultiDotValue
---
# InvalidMultiDotValue

**Severity Level: Error**

**Default state: Disabled**

## Description

PowerShell doesn't support unquoted literal values with multiple dots (`.`). Any value with two or
more dots results in `$null`, which can lead to unexpected behavior or errors in the code. This rule
identifies instances where such values are used.

To create values with multiple dots you must enclose the value in quotes and use type-casting or use
type constructor methods to create the appropriate object.

## Example

### Noncompliant

```powershell
$version = 1.2.3
```

or even:

```powershell
$IP = [System.Net.IPAddress]127.0.0.1
```

Where both examples result in `$null` instead of the expected value.

### Compliant

```powershell
# Use type-casting with quoted value
$IP = [System.Net.IPAddress]'127.0.0.1'
$version = [Version]'1.2.3'

# Use type constructor method
$version = [Version]::new(1, 2, 3)
```

## Configure rule

```powershell
Rules = @{
    PSInvalidMultiDotValue  = @{
        Enable = $true
    }
}
```

### Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.
