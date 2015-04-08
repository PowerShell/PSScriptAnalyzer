#AvoidUsingFilePath 
**Severity Level: Error**


##Description

If a file path is used in a script that refers to a file on the computer or on the shared network, this may expose information about your computer. Furthermore, the file path may not work on other computer when they try to use the script.

##How to Fix

Please change the path of the file to non-rooted.

##Example

Wrong： 
	
	Write-Warning "E:\Code"
	Get-ChildItem \\scratch2\scratch\


Correct:

	Get-ChildItem "..\Test"
