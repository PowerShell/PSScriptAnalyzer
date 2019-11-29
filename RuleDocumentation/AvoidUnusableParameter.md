# AvoidUnusableParameter

**Severity Level: Warning**

## Description

Parameter name should follow good naming practices, i.e. start with letter and contain only letters and digits. If parameter name cannot be parsed properly, then in certain scenarios, parsing can fail.

## Example

### Wrong

``` PowerShell
function Test
{
    Param
    (
        [switch]$1
    )

    if ($1) {'Yes'} else {'No'}
}

PS:\> Test -1 # not parsed as a switch
No
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

    if ($Parameter1) {'Yes'} else {'No'}
}

PS:\> Test -Parameter1 # parsed properly
Yes
```

### Splatting

Strictly speaking, these parameters are not prohibited. They are just not intuitive to follow, and hard to use. They can be used with splatting.
Using the first (not recommended) example, you can do this:
``` PowerShell
PS:\> $splat1 = @{'1'=$true}
PS:\> Test @splat1
Yes
```