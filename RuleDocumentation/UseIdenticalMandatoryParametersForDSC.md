# UseIdenticalMandatoryParametersForDSC

**Severity Level: Error**

## Description

For script based DSC resources the `Get-TargetResource`, `Test-TargetResource` and `Set-TargetResource` functions must have identical mandatory parameters that are also the keys in the corresponding `mof` file.

## How

Make sure all the keys have equivalent mandatory parameters in the `Get/Set/Test` functions.

## Example
Consider the following `mof` file.
```powershell
class WaitForAny : OMI_BaseResource
{
    [key, Description("Name of Resource on remote machine")]
    string Name;

    [required, Description("List of remote machines")]
    string NodeName[];
};
```

### Wrong

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
        $TargetName # this should be Name
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
