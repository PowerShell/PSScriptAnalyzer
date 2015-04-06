#AvoidTrapStatement
**Severity Level: Warning**


##Description

The Trap keyword specifies a list of statements to run when a terminating error occurs. It is designed for administrators. For script developers, you should use try-catch-finally statement.

##How to Fix

To fix a violation of this rule, please remove Trap statements and use try-catch-finally statement instead.

##Example

Wrong：    

    function TrapTest 
    {
    	trap {"Error found: $_"}
	}

Correct:    

    function TrapTest 
    {
    	try 
    	{
            $a = New-Object "dafdf"
            $a | get-member
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