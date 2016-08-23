#AvoidTrapStatement
**Severity Level: Warning**

##Description
The `Trap` keyword specifies a list of statements to run when a terminating error occurs.

Trap statements handle the terminating errors and allow execution of the script or function to continue instead of stopping.

Traps are intended for the use of administrators and not for script and cmdlet developers. PowerShell scripts and cmdlets should make use
of `try{} catch{} finally{}` statements.

##How to Fix
Replace `Trap` statements with `try{} catch{} finally{}` statements.

##Example
###Wrong：
``` PowerShell
function Test-Trap
{
    trap {"Error found: $_"}
}
```

###Correct:
``` PowerShell
function Test-Trap
{
    try
    {
        $a = New-Object "NonExistentObjectType"
        $a | Get-Member
    }
    catch [System.Exception]
    {
        "Found error"
    }
    finally
    {
        "End the script"
    }
    }
 ```
