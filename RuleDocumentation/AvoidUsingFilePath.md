#AvoidUsingFilePath
**Severity Level: Error**

##Description
If a file path is used in a script that refers to a file on the computer or on the shared network, this could expose sensitive information or result in availability issues.

Care should be taken to ensure that no computer or network paths are hard coded, instead non-rooted paths should be used.

##How to Fix
Ensure that no network paths are hard coded and that file paths are non-rooted.

##Example
###Wrong：
``` PowerShell
Function Get-MyCSVFile
{
	$FileContents = Get-FileContents -Path "\\scratch2\scratch\"
	...
}

Function Write-Documentation
{
	Write-Warning "E:\Code"
	...
}
```

###Correct:
``` PowerShell
Function Get-MyCSVFile ($NetworkPath)
{
	$FileContents = Get-FileContents -Path $NetworkPath
	...
}

Function Write-Documentation
{
	Write-Warning "..\Code"
	...
}
```
