Function Get-ExtentText
{
	Param(
	[Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent] $violation,
	[string] $scriptPath
	)
	$scriptContent = Get-Content -Path $scriptPath
	$start = [System.Management.Automation.Language.ScriptPosition]::new($scriptPath, $violation.StartLineNumber, $violation.StartColumnNumber, $scriptContent[$violation.StartLineNumber - 1])
	$end = [System.Management.Automation.Language.ScriptPosition]::new($scriptPath, $violation.EndLineNumber, $violation.EndColumnNumber, $scriptContent[$violation.EndLineNumber - 1])	
	$extent = [System.Management.Automation.Language.ScriptExtent]::new($start, $end)
	return($extent.Text)
}

Export-ModuleMember -Function Get-ExtentText