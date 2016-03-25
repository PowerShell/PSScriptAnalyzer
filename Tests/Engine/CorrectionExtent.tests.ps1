if (!(Get-Module PSScriptAnalyzer))
{
	Import-Module PSScriptAnalyzer
}


Describe "Correction Extent" {
	 Context "It should throw for invalid arguments" {
	 	 It "throw if end line number is less than start line number" {
		    $filename = "newfile"
		    $startLineNumber =  2
		    $endLineNumber = 1
		    $startColumnNumber = 1

		    $text = "Get-ChildItem"		    	      		    	      
		    $endColumnNumber = $text.Length
		    {[Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent]::new($filename, $startLineNumber, $startColumnNumber, $endLineNumber, $endColumnNumber, "T")} | Should Throw
		 }
	}
}


