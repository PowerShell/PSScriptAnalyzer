# AvoidUsingComputerNameHardcoded

**Severity Level: Error**

## Description

The names of computers should never be hard coded as this will expose sensitive information. The `ComputerName` parameter should never have a hard coded value.

## How

Remove hard coded computer names.

## Example

### Wrong

``` PowerShell
Function Invoke-MyRemoteCommand ()
{
	Invoke-Command -Port 343 -ComputerName "hardcoderemotehostname"
}
```

### Correct

``` PowerShell
Function Invoke-MyCommand ($ComputerName)
{
	Invoke-Command -Port 343 -ComputerName $ComputerName
}
```

## Example

### Wrong

``` PowerShell
Function Invoke-MyLocalCommand ()
{
	Invoke-Command -Port 343 -ComputerName "hardcodelocalhostname"
}
```

### Correct

``` PowerShell
Function Invoke-MyLocalCommand ()
{
	Invoke-Command -Port 343 -ComputerName $env:COMPUTERNAME
}
```
