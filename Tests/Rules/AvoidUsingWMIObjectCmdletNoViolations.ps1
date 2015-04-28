# No Rule violations since this script requires PS 2.0 and Get-CIMInstance is not available for this version
# So using Get-WMIObject is OK

#requires -Version 2.0

function TestFunction
{
    Remove-WmiObject -Class Win32_ComputerSystem

}

TestFunction

Get-WmiObject -Class Win32_OperatingSystem -Verbose