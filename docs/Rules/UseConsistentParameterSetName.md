---
description: Use consistent parameter set names and proper parameter set configuration
ms.date: 07/21/2026
ms.topic: reference
title: UseConsistentParameterSetName
---

# UseConsistentParameterSetName

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule detects inconsistent parameter set naming and configuration issues that can cause runtime
errors. Parameter set names in PowerShell are case-sensitive, unlike most other PowerShell elements.
With this rule enabled, it's easier to debug your functions. This rule is **disabled** by default.

This rule performs five different checks:

1. **Missing DefaultParameterSetName** - Warns when parameter sets are used but no default is
    specified.
1. **Multiple parameter declarations** - Detects when a parameter is declared multiple times in the
    same parameter set. This issue becomes a runtime exception, so this check catches it earlier.
1. **Case mismatch between DefaultParameterSetName and ParameterSetName** - Ensures consistent
    casing.
1. **Case mismatch between different ParameterSetName values** - Ensures all references to the same
    parameter set use identical casing.
1. **Parameter set names containing newlines** - Warns against using newline characters in parameter
    set names.

## Guidance

- Use a `DefaultParameterSetName` when defining multiple parameter sets
- Ensure consistent casing between `DefaultParameterSetName` and `ParameterSetName` values
- Use identical casing for all references to the same parameter set name
- Avoid declaring the same parameter multiple times in a single parameter set
- Don't use newline characters in parameter set names

### Notes

- The first occurrence of a parameter set name in your code is treated as the canonical casing
- Parameters without `[Parameter()]` attributes are automatically part of all parameter sets
- It's a PowerShell best practice to always specify a `DefaultParameterSetName` when you're using
  parameter sets

## Example

### Noncompliant

```powershell
# Missing DefaultParameterSetName
function Get-Data {
    [CmdletBinding()]
    param(
        [Parameter(ParameterSetName='ByName')]
        [string]$Name,

        [Parameter(ParameterSetName='ByID')]
        [int]$ID
    )
}

# Case mismatch between DefaultParameterSetName and ParameterSetName
function Get-Data {
    [CmdletBinding(DefaultParameterSetName='ByName')]
    param(
        [Parameter(ParameterSetName='byname')]
        [string]$Name,

        [Parameter(ParameterSetName='ByID')]
        [int]$ID
    )
}

# Inconsistent casing between ParameterSetName values
function Get-Data {
    [CmdletBinding(DefaultParameterSetName='ByName')]
    param(
        [Parameter(ParameterSetName='ByName')]
        [string]$Name,

        [Parameter(ParameterSetName='byname')]
        [string]$DisplayName
    )
}

# Multiple parameter declarations in same set
function Get-Data {
    param(
        [Parameter(ParameterSetName='ByName')]
        [Parameter(ParameterSetName='ByName')]
        [string]$Name
    )
}

# Parameter set name with newline
function Get-Data {
    param(
        [Parameter(ParameterSetName="Set`nOne")]
        [string]$Name
    )
}
```

### Compliant

```powershell
# Proper parameter set configuration
function Get-Data {
    [CmdletBinding(DefaultParameterSetName='ByName')]
    param(
        [Parameter(ParameterSetName='ByName', Mandatory)]
        [string]$Name,

        [Parameter(ParameterSetName='ByName')]
        [Parameter(ParameterSetName='ByID')]
        [string]$ComputerName,

        [Parameter(ParameterSetName='ByID', Mandatory)]
        [int]$ID
    )
}
```

## Configure rule

```powershell
Rules = @{
    PSUseConsistentParameterSetName  = @{
        Enable = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.
