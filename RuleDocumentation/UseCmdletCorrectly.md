#UseCmdletCorrectly 
**Severity Level: Error**


##Description

Cmdlets must be invoked with the correct syntax and parameters. For example, calling Set-Date with no parameters would be triggered by this rule since it has a required date parameter. 

##How to Fix

To fix a violation of this rule, please use mandatory parameters when calling cmdlets.

##Example

Wrong： 

	set-date

Correct: 

	$t = get-date
	set-date -date $t
