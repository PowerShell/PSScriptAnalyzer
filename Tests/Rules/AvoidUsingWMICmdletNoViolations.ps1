# No Rule violations since this script requires PS 2.0 and Get-CIMInstance is not available for this version
# So using Get-WMIObject is OK

#requires -Version 2.0

Invoke-WMIMethod -Path Win32_Process -Name Create -ArgumentList notepad.exe

function TestFunction
{
    Get-WmiObject -Class Win32_ComputerSystem  

    Register-WMIEvent -Class Win32_ProcessStartTrace -SourceIdentifier "ProcessStarted"

    Set-WMIInstance -Class Win32_Environment -Argument @{Name='MyEnvVar';VariableValue='VarValue';UserName='<SYSTEM>'}
}

TestFunction

Remove-WmiObject -Class Win32_OperatingSystem -Verbose