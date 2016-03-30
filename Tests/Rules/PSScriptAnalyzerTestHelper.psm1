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

Function Test-CorrectionExtent
{
	Param(
		[string] $violationFilepath,
		[Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord] $diagnosticRecord, 
		[int] $correctionsCount, 
		[string] $violationText, 
		[string] $correctionText
	)
	$corrections = $diagnosticRecord.SuggestedCorrections
	$corrections.Count | Should Be 1
	$corrections[0].Text | Should Be $correctionText
	Get-ExtentText $corrections[0] $violationFilepath | `
		       Should Be $violationText
}


Export-ModuleMember -Function Get-ExtentText
Export-ModuleMember -Function Test-CorrectionExtent