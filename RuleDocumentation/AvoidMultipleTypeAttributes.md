# AvoidMultipleTypeAttributes

**Severity Level: Warning**

## Description

Parameters should not have more than one type specifier. Multiple type specifiers on parameters will cause a runtime error.

## How

Ensure each parameter has only 1 type specifier.

## Example

### Wrong

``` PowerShell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Param1,

        [switch]
        [bool]
        $Switch
    )
    ...
}
```

### Correct

``` PowerShell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Param1,

        [switch]
        $Switch
    )
    ...
}
```
