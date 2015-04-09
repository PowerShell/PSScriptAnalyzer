#UsePSCredentialType
**Severity Level: Warning**


##Description

Checks that cmdlets that have a Credential parameter accept PSCredential. This comes from the PowerShell teams best practices.

##How to Fix

Please change the parameter type to be PSCredential.

##Example

Wrong:

	function Credential([String]$Credential) 
	{
		...
	}

Correct:

	function Credential([PSCredential]$Credential) 
	{
		...
	}
