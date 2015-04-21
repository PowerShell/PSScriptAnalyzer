#Script violates the rule because Get-CIMInstance is available on PS 3.0 and needs to use that

#requires -version 3.0

function TestFunction
{
    Get-WmiObject -Class Win32_ComputerSystem

}

TestFunction

Get-WmiObject -Class Win32_OperatingSystem -Verbose