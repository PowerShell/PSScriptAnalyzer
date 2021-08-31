# AvoidMultipleTypeAttributes

**Severity Level: Warning**

## Description

Parameters should not have more than one type specifier. Multiple type specifiers on parameters
cause runtime errors.

## How

Ensure each parameter has only 1 type specifier.

## Example

### Wrong

```powershell
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

```powershell
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
