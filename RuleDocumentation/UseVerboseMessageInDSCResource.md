# UseVerboseMessageInDSCResource

**Severity Level: Information**

## Description

Best practice recommends that additional user information is provided within commands, functions and scripts using `Write-Verbose`.

## How

Make use of the `Write-Verbose` command.

## Example

### Wrong

``` PowerShell
Function Test-Function
{
    [CmdletBinding()]
    Param()
    ...
}
```

### Correct

``` PowerShell
Function Test-Function
{
    [CmdletBinding()]
    Param()
    Write-Verbose "Verbose output"
    ...
}
```
