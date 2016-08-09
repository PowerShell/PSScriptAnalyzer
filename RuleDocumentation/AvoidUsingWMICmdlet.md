#AvoidUsingWMICmdlet
**Severity Level: Warning**

##Description
As of PowerShell 3.0, the CIM CMDLets should be used over the WMI CMDLets.

The following CMDLets should not be used:
    - ```Get-WmiObject```
    - ```Remove-WmiObject```
    - ```Invoke-WmiObject```
    - ```Register-WmiEvent```
    - ```Set-WmiInstance```

Use the following CMDLets instead:
    - ```Get-CimInstance```
    - ```Remove-CimInstance```
    - ```Invoke-CimMethod```
    - ```Register-CimIndicationEvent```
    - ```Set-CimInstance```

The CIM CMDLets comply with WS-Management (WSMan) standards and with the Common Information Model (CIM) standard, allowing for the management of Windows and non-Windows operating systems.

##How to Fix
Change to the equivalent CIM based CMDLet.
    - ```Get-WmiObject``` -> ```Get-CimInstance```
    - ```Remove-WmiObject``` -> ```Remove-CimInstance```
    - ```Invoke-WmiObject``` -> ```Invoke-CimMethod```
    - ```Register-WmiEvent``` -> ```Register-CimIndicationEvent```
    - ```Set-WmiInstance``` -> ```Set-CimInstance```

##Example
###Wrong:
``` PowerShell
Get-WmiObject -Query 'Select * from Win32_Process where name LIKE "myprocess%"' | Remove-WmiObject
Invoke-WmiMethod –Class Win32_Process –Name "Create" –ArgumentList @{ CommandLine = "notepad.exe" }
```

###Correct:
``` PowerShell
Get-CimInstance -Query 'Select * from Win32_Process where name LIKE "myprocess%"' | Remove-CIMInstance
Invoke-CimMethod –ClassName Win32_Process –MethodName "Create" –Arguments @{ CommandLine = "notepad.exe" }
```
