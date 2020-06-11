Function Get-ExtentTextFromContent
{
	    Param(
	[Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent] $violation,
	[string] $rawContent
	)
	$scriptContent = New-Object -TypeName 'System.Collections.ArrayList'
	$stringReader = New-Object -TypeName 'System.IO.StringReader' -ArgumentList @($rawContent)
	while ($stringReader.Peek() -ne -1)
	{
		$scriptContent.Add($stringReader.ReadLine()) | Out-Null
	}

	$typeScriptPos = 'System.Management.Automation.Language.ScriptPosition'
	$start = New-Object -TypeName $typeScriptPos -ArgumentList @($scriptPath, $violation.StartLineNumber, $violation.StartColumnNumber, $scriptContent[$violation.StartLineNumber - 1])
	$end = New-Object -TypeName $typeScriptPos -ArgumentList @($scriptPath, $violation.EndLineNumber, $violation.EndColumnNumber, $scriptContent[$violation.EndLineNumber - 1])
	$extent = New-Object -TypeName 'System.Management.Automation.Language.ScriptExtent' -ArgumentList @($start, $end)
	$extent.Text
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

	Test-CorrectionExtentFromContent (Get-Content $violationFilepath -Raw) `
		$diagnosticRecord `
		$correctionsCount `
		$violationText `
		$correctionText
}

Function Test-CorrectionExtentFromContent {
    param(
        [string] $rawContent,
        [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord] $diagnosticRecord,
        [ValidateRange(0, 1)]
        [int] $correctionsCount,
        [string] $violationText,
        [string] $correctionText
    )

	$corrections = $diagnosticRecord.SuggestedCorrections
	$corrections.Count | Should -Be $correctionsCount
	$corrections[0].Text | Should -Be $correctionText
	Get-ExtentTextFromContent $corrections[0] $rawContent | `
		       Should -Be $violationText
}

Function Get-Count
{
	Begin {$count = 0}
	Process {$count++}
	End {$count}
}

Export-ModuleMember -Function Get-ExtentText
Export-ModuleMember -Function Test-CorrectionExtent
Export-ModuleMember -Function Test-CorrectionExtentFromContent
Export-ModuleMember -Function Get-Count
