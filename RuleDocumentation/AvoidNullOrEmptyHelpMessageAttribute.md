#AvoidNullOrEmtpyHelpMessageAttribute
**Severity Level: Warning**


##Description

Setting the HelpMessage attribute to an empty string or null value causes PowerShell interpreter to throw an error while executing the corresponding function.

##How to Fix

To fix a violation of this rule, please set its value to a non-empty string.

##Example

Wrong:

Function BadFuncEmtpyHelpMessageEmpty
{
	Param(
		[Parameter(HelpMessage='')]
		[String] $Param
	)

	$Param
}

Function BadFuncEmtpyHelpMessageNull
{
	Param(
		[Parameter(HelpMessage=$null)]
		[String] $Param
	)

	$Param
}

Function BadFuncEmtpyHelpMessageNoAssignment
{
	Param(
		[Parameter(HelpMessage)]
		[String] $Param
	)

	$Param
}


Correct:

Function GoodFuncEmtpyHelpMessage
{
	Param(
		[Parameter(HelpMessage='This is helpful')]
		[String] $Param
	)

	$Param
}
