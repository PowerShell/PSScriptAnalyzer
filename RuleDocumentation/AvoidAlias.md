#AvoidAlias 
**Severity Level: Warning**


##Description

An alias is an alternate name or nickname for a cmdlet or for a command element, such as a function, script, file, or executable file. But when writing scripts that will potentially need to be maintained over time, either by the original author or another Windows PowerShell scripter, please consider using full cmdlet name instead of alias. Aliases can introduce these problems, readability, understandability and availability.

##How to Fix

Please consider using full cmdlet name instead of alias. 

##Example

Wrong： gps | where-object {$_.WorkingSet -gt 20000000}

Correct: get-process | where-object {$_.WorkingSet -gt 20000000}
