---
description: Use Identical Parameters For DSC Test and Set Functions
ms.custom: PSSA v1.21.0
ms.date: 10/18/2021
ms.topic: reference
title: DSCUseIdenticalParametersForDSC
---
# UseIdenticalParametersForDSC

**Severity Level: Error**

## Description

The `Get-TargetResource`, `Test-TargetResource` and `Set-TargetResource` functions of DSC Resource
must have the same parameters.

## How

Correct the parameters for the functions in DSC resource.

## Example

### Wrong

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

### Correct

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
