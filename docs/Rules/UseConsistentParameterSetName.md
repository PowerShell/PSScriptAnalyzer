---
description: Use consistent parameter set names and proper parameter set configuration.
ms.date: 08/19/2025
ms.topic: reference
title: UseConsistentParameterSetName
---

# UseConsistentParameterSetName

**Severity Level: Warning**

## Description

Parameter set names in PowerShell are case-sensitive, unlike most other PowerShell elements. This rule ensures consistent casing and proper configuration of parameter sets to avoid runtime errors and improve code clarity.

The rule performs five different checks:

1. **Missing DefaultParameterSetName** - Warns when parameter sets are used but no default is specified
2. **Multiple parameter declarations** - Detects when a parameter is declared multiple times in the same parameter set. This is ultimately a runtime exception - this check helps catch it sooner.
3. **Case mismatch between DefaultParameterSetName and ParameterSetName** - Ensures consistent casing
4. **Case mismatch between different ParameterSetName values** - Ensures all references to the same parameter set use identical casing
5. **Parameter set names containing newlines** - Warns against using newline characters in parameter set names

## How

- Use a `DefaultParameterSetName` when defining multiple parameter sets
- Ensure consistent casing between `DefaultParameterSetName` and `ParameterSetName` values
- Use identical casing for all references to the same parameter set name
- Avoid declaring the same parameter multiple times in a single parameter set
- Do not use newline characters in parameter set names

## Example

### Wrong

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

### Correct

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

## Notes

- Parameter set names are case-sensitive in PowerShell, making this different from most other PowerShell elements
- The first occurrence of a parameter set name in your code is treated as the canonical casing
- Parameters without [Parameter()] attributes are automatically part of all parameter sets
- It's a PowerShell best practice to always specify a DefaultParameterSetName when using parameter sets