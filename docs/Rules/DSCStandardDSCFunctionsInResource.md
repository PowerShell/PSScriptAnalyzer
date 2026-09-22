---
description: Use standard DSC Get, Set, and Test TargetResource functions in a resource
ms.date: 07/21/2026
ms.topic: reference
title: DSCStandardDSCFunctionsInResource
---
# StandardDSCFunctionsInResource

**Severity Level: Error**

**Default state: Always enabled**

## Description

This rule detects if all Desired State Configuration (DSC) resources implement the correct
functions. Add the missing functions to the resource.

For non-class based resources:

- `Get-TargetResource`
- `Set-TargetResource`
- `Test-TargetResource`

For class based resources:

- `Get`
- `Set`
- `Test`

## Example

### Noncompliant MOF-based resource

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
