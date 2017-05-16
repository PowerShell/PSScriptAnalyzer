# UseIdenticalParametersForDSC

**Severity Level: Error**

## Description

The `Get-TargetResource`, `Test-TargetResource` and `Set-TargetResource` functions of DSC Resource must have the same parameters.

## How

Correct the parameters for the functions in DSC resource.

## Example

### Wrong

``` PowerShell
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

``` PowerShell
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
