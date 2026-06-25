---
description: Use identical parameters for DSC Get, Set, and Test TargetResource functions in a resource
ms.date: 06/03/2026
ms.topic: reference
title: DSCUseIdenticalParametersForDSC
---
# UseIdenticalParametersForDSC

**Severity Level: Error**

## Description

This rule detects if the `Get-TargetResource`, `Set-TargetResource`, and `Test-TargetResource`
functions in a Desired State Configuration (DSC) resource all have identical parameters. If they
don't match, you'll need to update the function parameters to be consistent across all three
functions.

## Example

### Noncompliant

```powershell
function Get-TargetResource
{
    [OutputType([Hashtable])]
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name,

        [String]
        $TargetResource
    )
    ...
}

function Set-TargetResource
{
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name
    )
    ...
}

function Test-TargetResource
{
    [OutputType([System.Boolean])]
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name
    )
    ...
}
```

### Compliant

```powershell
function Get-TargetResource
{
    [OutputType([Hashtable])]
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name,

        [String]
        $TargetResource
    )
    ...
}

function Set-TargetResource
{
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name,

        [String]
        $TargetResource
    )
    ...
}

function Test-TargetResource
{
    [OutputType([System.Boolean])]
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name,

        [String]
        $TargetResource
    )
    ...
}
```
