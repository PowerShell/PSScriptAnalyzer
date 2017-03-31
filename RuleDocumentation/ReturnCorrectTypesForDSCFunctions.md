# ReturnCorrectTypesForDSCFunctions

**Severity Level: Information**

## Description

The functions in DSC resources have specific return objects.

For non-class based resources:
* `Set-TargetResource` must not return any value.
* `Test-TargetResource` must return a boolean.
* `Get-TargetResource` must return a hash table.

For class based resources:
* `Set` must not return any value.
* `Test` must return a boolean.
* `Get` must return an instance of the DSC class.

## How

Ensure that each function returns the correct type.

## Example

### Wrong

``` PowerShell
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

### Correct

``` PowerShell
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

## Example

### Wrong

``` PowerShell
[DscResource()]
class MyDSCResource
{
    [DscProperty(Key)]
    [string] $Name

    [String] Get()
    {
        ...
    }

    [String] Set()
    {
        ...
    }

    [bool] Test()
    {
        ...
    }
}
```

### Correct

``` PowerShell
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
