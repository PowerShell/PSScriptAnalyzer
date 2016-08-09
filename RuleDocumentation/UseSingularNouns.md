#UseSingularNouns 
**Severity Level: Warning**

##Description
PowerShell team best practices state CMDLets should use singular nouns and not plurals.

##How to Fix
Change plurals to singular.

##Example
###Wrong：
``` PowerShell
function Get-Files
{
	...
}
```

###Correct: 
``` PowerShell
function Get-File
{
	...
}
```
