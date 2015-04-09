#ProvideVerboseMessage 
**Severity Level: Warning**


##Description

Checks that Write-Verbose is called at least once in every cmdlet or script. This is in line with the PowerShell best practices.

##How to Fix

Please consider adding Write-Verbose in each cmdlet.

##Example
Correct
```
Function TestFunction1
{
    [cmdletbinding()]
    Param()
    Write-Verbose "Verbose output"

}

Function TestFunction2
{
    [cmdletbinding()]
    Param(）
    Write-Verbose "Verbose output"
}
```
