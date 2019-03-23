function Test-X
{
    [CmdletBinding()]
    param(
	[Parameter(ValueFromPipeline)]
	[string[]]
	$Str
    )

    begin
    {
	if (($null -eq $Str) -or $Str.Length -eq 0)
	{
	    $Str = @('Hello')
	}

	Write-Host "`$Str = $Str, `$Str.Length = $($Str.Length)"
    }

    process
    {
	Write-Output $Str
    }
}
