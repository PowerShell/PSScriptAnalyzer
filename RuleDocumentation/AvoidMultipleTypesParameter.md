# AvoidMultipleTypesParameter

**Severity Level: Warning**

## Description

Parameter should not have more than one type specifier.

## How

Make each parameter has only 1 type spcifier.

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
        $Switch=$True
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
        $Switch=$False
    )
    ...
}
```
