Function Get-ExtentText
{
	Param(
	[Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent] $violation,
	[string] $scriptPath
	)
	$scriptContent = Get-Content -Path $scriptPath
	$typeScriptPos = 'System.Management.Automation.Language.ScriptPosition'
	$start = New-Object -TypeName $typeScriptPos -ArgumentList @($scriptPath, $violation.StartLineNumber, $violation.StartColumnNumber, $scriptContent[$violation.StartLineNumber - 1])
	$end = New-Object -TypeName $typeScriptPos -ArgumentList @($scriptPath, $violation.EndLineNumber, $violation.EndColumnNumber, $scriptContent[$violation.EndLineNumber - 1])
	$extent = New-Object -TypeName 'System.Management.Automation.Language.ScriptExtent' -ArgumentList @($start, $end)
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
	$corrections.Count | Should Be $correctionsCount
	$corrections[0].Text | Should Be $correctionText
	Get-ExtentText $corrections[0] $violationFilepath | `
		       Should Be $violationText
}

Function Test-PSEditionCoreCLR
{
    [bool]$IsCoreCLR
}

Function Test-PSEditionCoreCLRLinux
{
    (Test-PSEditionCoreCLR) -and $IsLinux
}

Function Get-Count
{
	Begin {$count = 0}
	Process {$count++}
	End {$count}
}

Export-ModuleMember -Function Get-ExtentText
Export-ModuleMember -Function Test-CorrectionExtent
Export-ModuleMember -Function Test-PSEditionCoreCLR
Export-ModuleMember -Function Test-PSEditionCoreCLRLinux
Export-ModuleMember -Function Get-Count