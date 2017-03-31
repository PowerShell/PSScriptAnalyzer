# ReservedParams

**Severity Level: Error**

## Description

You cannot use reserved common parameters in an advanced function.

## How

Change the name of the parameter.

## Example

### Wrong

``` PowerShell
function Test
{
    [CmdletBinding]
    Param
    (
        $ErrorVariable,
        $Parameter2
    )
}
```

### Correct

``` PowerShell
function Test
{
    [CmdletBinding]
    Param
    (
        $Err,
        $Parameter2
    )
}
```
