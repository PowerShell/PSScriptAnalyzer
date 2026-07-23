---
description: Return correct types for DSC functions
ms.date: 07/21/2026
ms.topic: reference
title: DSCReturnCorrectTypesForDSCFunctions
---
# ReturnCorrectTypesForDSCFunctions

**Severity Level: Information**

**Default state: Always enabled**

## Description

This rule detects if functions in Desired State Configuration (DSC) resources have specific return
objects. You'll need to ensure that each function returns the correct type.

For non-class based resources:

- `Get-TargetResource` must return a hash table.
- `Set-TargetResource` must not return any value.
- `Test-TargetResource` must return a boolean.

For class based resources:

- `Get` must return an instance of the DSC class.
- `Set` must not return any value.
- `Test` must return a boolean.

## Example

### Noncompliant MOF-based resource

```powershell
function Get-TargetResource
{
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
    param
    (
        [parameter(Mandatory = $true)]
        [String]
        $Name
    )
    ...
}
```

### Compliant MOF-based resource

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

### Noncompliant class-based resource

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
```

### Compliant class-based resource

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md