#UseSingularNouns 
**Severity Level: Warning**

##Description

Cmdlet should use singular instead of plural nouns. This comes from the PowerShell teams best practices.

##How to Fix

Please correct the plural nouns to be singluar.

##Example

Wrong：

	function Get-Files
	{
		...
	}

Correct: 

	function Get-File
	{
		...
	}
