#Script violates the rule because Get-CIMInstance is available on PS 3.0 and needs to use that

#requires -version 3.0

function TestFunction
{
    Get-WmiObject -Class Win32_ComputerSystem

    Invoke-WMIMethod -Path Win32_Process -Name Create -ArgumentList notepad.exe

    Register-WMIEvent -Class Win32_ProcessStartTrace -SourceIdentifier "ProcessStarted"

    Set-WMIInstance -Class Win32_Environment -Argument @{Name='MyEnvVar';VariableValue='VarValue';UserName='<SYSTEM>'}
}

TestFunction

Remove-WmiObject -Class Win32_OperatingSystem -Verbose