# ShouldProcess

**Severity Level: Warning**

## Description

If a cmdlet declares the `SupportsShouldProcess` attribute, then it should also call `ShouldProcess`. A violation is any function which either declares `SupportsShouldProcess` attribute but makes no calls to `ShouldProcess` or it calls `ShouldProcess` but does not declare `SupportsShouldProcess`

For more information, please refer to `about_Functions_Advanced_Methods` and `about_Functions_CmdletBindingAttribute`

## How

To fix a violation of this rule, please call `ShouldProcess` method when a cmdlet declares `SupportsShouldProcess` attribute. Or please add `SupportsShouldProcess` attribute argument when calling `ShouldProcess`

## Example

### Wrong

``` PowerShell
	function Set-File
	{
	    [CmdletBinding(SupportsShouldProcess=$true)]
	    Param
	    (
	        # Path to file
			[Parameter(Mandatory=$true)]
	        $Path
	    )
		"String" | Out-File -FilePath $FilePath
	}
```

### Correct

``` PowerShell
	function Set-File
	{
	    [CmdletBinding(SupportsShouldProcess=$true)]
	    Param
	    (
	        # Path to file
			[Parameter(Mandatory=$true)]
	        $Path
	    )

		if ($PSCmdlet.ShouldProcess("Target", "Operation"))
		{
			"String" | Out-File -FilePath $FilePath
		}
		else
		{
			Write-Host ('Write "String" to file {0}' -f $FilePath)
		}
	}
```
