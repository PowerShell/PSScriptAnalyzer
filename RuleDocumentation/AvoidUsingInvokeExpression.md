#AvoidUsingInvokeExpression
**Severity Level: Warning**


##Description

The Invoke-Expression cmdlet evaluates or runs a specified string as a command and returns the results of the expression or command. It can be extraordinarily powerful so it is not that you want to never use it but you need to be very careful about using it.  In particular, you are probably on safe ground if the data only comes from the program itself.  If you include any data provided from the user - you need to protect yourself from Code Injection. 


##How to Fix

To fix a violation of this rule, please remove Invoke-Expression from script and find other options instead.

##Example

Wrong： 

	Invoke-Expression "get-process"

Correct: 

	Get-process
