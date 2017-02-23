# AvoidUsingWriteHost

**Severity Level: Warning**

## Description

The use of `Write-Host` is greatly discouraged unless in the use of commands with the `Show` verb. The `Show` verb explicitly means "show on the screen, with no
other possibilities".

Commands with the `Show` verb do not have this check applied.

## How

Replace `Write-Host` with `Write-Output` or `Write-Verbose`.

## Example

### Wrong

``` PowerShell
function Test
{
	...
	Write-Host "Executing.."
	...
}
```

### Correct

``` PowerShell
function Test
{
	...
	Write-Output "Executing.."
	...
}

function Show-Something
{
    Write-Host "show something on screen";
}
```
