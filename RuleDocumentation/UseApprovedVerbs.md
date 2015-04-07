#UseApprovedVerbs 
**Severity Level: Warning**


##Description

All defined cmdlets must use approved verbs. This is in line with PowerShell's best practices.

##How to Fix

Please consider using full cmdlet name instead of alias. 

##Example

Wrong： 

    function Change-Item
    {
        ...
    }

Correct: 
    
    function Update-Item
    {
        ...
    }

