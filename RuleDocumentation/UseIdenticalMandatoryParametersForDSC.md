# UseIdenticalMandatoryParametersForDSC

**Severity Level: Error**

## Description

For script based DSC resources the `Get-TargetResource`, `Test-TargetResource` and `Set-TargetResource` functions:

1. If a parameter is declared as `mandatory` in any of the `Get/Set/Test` functions, then it should be a mandatory parameter in all the three functions.
1. If a property is declared with attributes `Key` of `Required` in a mof file, then is should be present as a mandatory parameter in the `Get/Set/Test` functions of the corresponding resource file.

## How

1. Make sure `Get/Set/Test` declare identical mandatory parameters.
1. Make sure all the properties with `Key` and `Required` attributes have equivalent mandatory parameters in the `Get/Set/Test` functions.

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
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message
    )
}

function Set-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}

function Test-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}
```

### Correct

``` PowerShell
function Get-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}

function Set-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}

function Test-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}
```
