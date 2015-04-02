#AvoidAlias

##Description

An alias is an alternate name or nickname for a cmdlet or for a command element, such as a function, script, file, or executable file. But when writing scripts that will potentially need to be maintained over time, either by the original author or another Windows PowerShell scripter, please consider using full cmdlet name instead of alias. Aliases can introduce these problems, readability, understandability and availability.

**Severity Level: Warning**

##Example

gps | where-object {$_.WorkingSet -gt 20000000}

cls

##How to Fix
Please consider using full cmdlet name instead of alias. 