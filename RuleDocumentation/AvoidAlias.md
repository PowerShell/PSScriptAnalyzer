#AvoidAlias 
**Severity Level: Warning**


##Description

An alias is an alternate name or nickname for a cmdlet or for a command element, such as a function, script, file, or executable file. 
When writing PowerShell that will potentially need to be maintained over time, either by the original author or others, please consider using full cmdlet name instead of alias. 

Aliases can introduce problems with readability, understandability and availability.

##How to Fix

Please consider using full cmdlet name instead of alias. 

##Example

Wrong:
``` PowerShell
gps | Where-Object {$_.WorkingSet -gt 20000000}
```

Correct:
``` PowerShell
Get-Process | Where-Object {$_.WorkingSet -gt 20000000}
```
