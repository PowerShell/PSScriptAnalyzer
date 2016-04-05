if (!(Get-Module PSScriptAnalyzer))
{
	Import-Module PSScriptAnalyzer
}

Describe "Correction Extent" {
	 $type = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent]

	 Context "Object construction" {
	 	 It "creates the object with correct properties" {
		    $correctionExtent = $type::new(1, 1, 1, 3, "get-childitem", "newfile", "cool description")

		    $correctionExtent.StartLineNumber | Should Be 1
		    $correctionExtent.EndLineNumber | Should Be 1
		    $correctionExtent.StartColumnNumber | Should Be 1
		    $correctionExtent.EndColumnNumber | Should be 3
		    $correctionExtent.Text | Should Be "get-childitem"
		    $correctionExtent.File | Should Be "newfile"
		    $correctionExtent.Description | Should Be "cool description"
		 }
	 
	 	 It "throws if end line number is less than start line number" {
		    $text = "Get-ChildItem"		    	      		    	      
		    {$type::new(2, 1, 1, $text.Length + 1, $text, "newfile")} | Should Throw "start line number"
		 }
		 
		 It "throws if end column number is less than start column number for same line" {
		    $text = "start-process"
		    {$type::new(1, 1, 2, 1, $text, "newfile")} | Should Throw "start column number"
		 }
	}
}


