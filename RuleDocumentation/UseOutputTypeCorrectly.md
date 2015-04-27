#UseOutputTypeCorrectly 
**Severity Level: Warning**


##Description

If a return type is declared, the cmdlet must return that type. If a type is returned, a return type must be declared.

##How to Fix

To fix a violation of this rule, please check the OuputType attribute lists and the types that are returned in your code. You can get more details by running “Get-Help about_Functions_OutputTypeAttribute” command in Windows PowerShell. 

##Example

##Example
Wrong:

	function Get-Foo
	{
	    [CmdletBinding()]
            [OutputType([String])]
            Param(
            )
            return "4
	}

Correct:

	function Get-Foo
	{
	    [CmdletBinding()]
            [OutputType([String])]
            Param(
            )

            return "bar"
	}
