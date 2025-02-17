---
description: Use Standard Get/Set/Test TargetResource functions in DSC Resource
ms.date: 06/28/2023
ms.topic: reference
title: DSCStandardDSCFunctionsInResource
---
# StandardDSCFunctionsInResource

**Severity Level: Error**

## Description

All DSC resources are required to implement the correct functions.

For non-class based resources:

- `Set-TargetResource`
- `Test-TargetResource`
- `Get-TargetResource`

For class based resources:

- `Set`
- `Test`
- `Get`

## How

Add the missing functions to the resource.

## Example 1

### Wrong

```powershell
function Get-TargetResource
{
    [OutputType([Hashtable])]
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name
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
        $Name
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

## Example 2

### Wrong

```powershell
[DscResource()]
class MyDSCResource
{
    [DscProperty(Key)]
    [string] $Name

    [void] Set()
    {
        ...
    }

    [bool] Test()
    {
        ...
    }
}

### Correct

```powershell
[DscResource()]
class MyDSCResource
{
    [DscProperty(Key)]
    [string] $Name

    [MyDSCResource] Get()
    {
        ...
    }

    [void] Set()
    {
        ...
    }

    [bool] Test()
    {
        ...
    }
}
```
