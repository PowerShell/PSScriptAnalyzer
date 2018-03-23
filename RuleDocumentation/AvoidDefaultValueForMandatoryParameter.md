# AvoidDefaultValueForMandatoryParameter

**Severity Level: Warning**

## Description

Mandatory parameters should not have a default values because there is no scenario where the default can be used because `PowerShell` will prompt anyway if the parameter value is not specified when calling the function.

## Example

### Wrong

``` PowerShell
function Test
{

    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        $Parameter1 = 'default Value'
    )
}
```

### Correct

``` PowerShell
function Test
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        $Parameter1
    )
}
```
