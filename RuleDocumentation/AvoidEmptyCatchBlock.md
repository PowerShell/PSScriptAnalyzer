#AvoidEmptyCatchBlock 
**Severity Level: Warning**

##Description

Empty catch blocks are considered poor design decisions because if an error occurs in the try block, this error is simply swallowed and not acted upon. While this does not inherently lead to bad things. It can and this should be avoided if possible. 

##How to Fix

To fix a violation of this rule, using Write-Error or throw statements in catch blocks.

##Example
Wrong:

	try
	{
	    1/0
	}
	catch [DivideByZeroException]
	{
	}

Correct:

	try
	{
	    1/0
	}
	catch [DivideByZeroException]
	{
		Write-Error "DivideByZeroException"
	}
