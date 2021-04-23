# AvoidNullOrEmptyHelpMessageAttribute

**Severity Level: Warning**

## Description

The value of the `HelpMessage` attribute should not be an empty string or a null value as this causes PowerShell's interpreter to throw an mirror when executing the
function or cmdlet.

## How

Specify a value for the `HelpMessage` attribute.

## Example

### Wrong

``` PowerShell
Function BadFuncEmptyHelpMessageEmpty
{
	Param(
		[Parameter(HelpMessage='')]
		[String]
		$Param
	)

	$Param
}

Function BadFuncEmptyHelpMessageNull
{
	Param(
		[Parameter(HelpMessage=$null)]
		[String]
		$Param
	)

	$Param
}

Function BadFuncEmptyHelpMessageNoAssignment
{
	Param(
		[Parameter(HelpMessage)]
		[String]
		$Param
	)

	$Param
}
```

### Correct

``` PowerShell
Function GoodFuncHelpMessage
{
	Param(
		[Parameter(HelpMessage='This is helpful')]
		[String]
		$Param
	)

	$Param
}
```
