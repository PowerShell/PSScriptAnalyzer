#AvoidUsingWMICmdlet
**Severity Level: Warning**


##Description

Avoid Using Get-WMIObject, Remove-WMIObject, Invoke-WmiMethod, Register-WmiEvent, Set-WmiInstance

For PowerShell 3.0 and above, use CIM cmdlet which perform the same tasks as the WMI cmdlets. The CIM cmdlets comply with WS-Management (WSMan) standards and with the Common Information Model (CIM) standard, which enables the cmdlets to use the same techniques to manage Windows computers and those running other operating systems.

##How to Fix

Use corresponding CIM cmdlets such as Get-CIMInstance, Remove-CIMInstance, Invoke-CIMMethod, Register-CimIndicationEvent, Set-CimInstance 

##Example

Wrong:
Get-WmiObject -Query 'Select * from Win32_Process where name LIKE "myprocess%"' | Remove-WmiObject
Invoke-WmiMethod –Class Win32_Process –Name "Create" –ArgumentList @{ CommandLine = "notepad.exe" }

Correct:
Get-CimInstance -Query 'Select * from Win32_Process where name LIKE "myprocess%"' | Remove-CIMInstance
Invoke-CimMethod –ClassName Win32_Process –MethodName "Create" –Arguments @{ CommandLine = "notepad.exe" }
