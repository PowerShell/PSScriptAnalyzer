#UseCmdletCorrectly 
**Severity Level: Warning**


##Description

Cmdlets must be invoked with the correct syntax and parameters. For example, calling Set-Date with no parameters would be triggered by this rule since it has a required date parameter. 

##How to Fix

To fix a violation of this rule, please use mandatory parameters when calling cmdlets.

##Example

Wrong： 

	Set-Date

Correct: 

	$date = Get-Date
	Set-Date -Date $t
