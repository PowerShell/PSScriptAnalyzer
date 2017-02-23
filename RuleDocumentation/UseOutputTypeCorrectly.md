# UseOutputTypeCorrectly

**Severity Level: Information**

## Description

A command should return the same type as declared in `OutputType`.

You can get more details by running `Get-Help about_Functions_OutputTypeAttribute` command in Windows PowerShell.

## How

Specify that the OutputType attribute lists and the types returned in the cmdlet match.

## Example

### Wrong

``` PowerShell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )
        return 4
}
```

### Correct

``` PowerShell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )

        return "four"
}
```
