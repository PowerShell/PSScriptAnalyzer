# UseCmdletCorrectly

**Severity Level: Warning**

## Description

Whenever we call a command, care should be taken that it is invoked with the correct syntax and parameters.

## How

Specify all mandatory parameters when calling commands.

## Example

### Wrong

``` PowerShell
Function Set-TodaysDate ()
{
	Set-Date
	...
}
```

### Correct

``` PowerShell
Function Set-TodaysDate ()
{
	$date = Get-Date
	Set-Date -Date $date
	...
}
```
