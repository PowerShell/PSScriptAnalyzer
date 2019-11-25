# AvoidUnusableParameter

**Severity Level: Warning**

## Description

Parameter name should follow good naming practices. If parameter name cannot be parsed properly, then it can not be used in a standard way.

## Example

### Wrong

``` PowerShell
function Test
{

    [CmdletBinding()]
    Param
    (
        [switch]$1
    )

    if ($1) {Write-Output "1"}
}

Test -1 # parsed as integer "minus one", not as switch parameter
```

### Correct

``` PowerShell
function Test
{
    [CmdletBinding()]
    Param
    (
        [switch]$Parameter1
    )

    if ($Parameter1) {Write-Output "1"}
}

Test -Parameter1 # parsed properly
```
