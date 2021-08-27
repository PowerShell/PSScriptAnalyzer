# AvoidMultipleTypeAttributes

**Severity Level: Warning**

## Description

Parameters should not have more than one type specifier. Multiple type specifiers on parameters will cause a runtime error.

## How

Ensure each parameter has only 1 type spcifier.

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
        [boolean]
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
