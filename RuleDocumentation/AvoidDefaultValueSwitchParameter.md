# AvoidDefaultValueSwitchParameter

**Severity Level: Warning**

## Description

Switch parameters for commands should default to false.

## How

Change the default value of the switch parameter to be false.

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
