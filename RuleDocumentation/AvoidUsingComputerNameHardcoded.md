#AvoidUsingComputerNameHardcoded 
**Severity Level: Error**


##Description

The ComputerName parameter of a cmdlet should not be hardcoded as this will expose sensitive information about the system.

##How to Fix

Please consider using full cmdlet name instead of alias. 

##Example

Wrong： 

	Invoke-Command -Port 343 -ComputerName "hardcode1"
	Invoke-Command -ComputerName:"hardcode2"


Correct: 

	Invoke-Command -ComputerName $comp
	Invoke-Command -ComputerName $env:COMPUTERNAME
