Import-Module PSScriptAnalyzer

Describe "IScriptExtent Extension Methods" {
    Context "When single line inner extent is strictly contained in a single line outer extent" {
        It "Should return true" {
           $outer = "This is the outer string"
           $outerExtentStartPos = New-Object -TypeName 'System.Management.Automation.Language.ScriptPosition' -ArgumentList $null,1,1,$outer,$outer
           $outerExtentEndPos = New-Object -TypeName 'System.Management.Automation.Language.ScriptPosition' -ArgumentList $null,1,$outer.Length,$outer,$outer
           $outerExtent = New-Object -TypeName 'System.Management.Automation.Language.ScriptExtent' -ArgumentList $outerExtentStartPos,$outerExtentEndPos

           $innerExtentStartPos = New-Object -TypeName 'System.Management.Automation.Language.ScriptPosition' -ArgumentList $null,1,1,$outer,$outer
           $innerExtentEndPos = New-Object -TypeName 'System.Management.Automation.Language.ScriptPosition' -ArgumentList $null,1,2,$outer,$outer
           $innerExtent = New-Object -TypeName 'System.Management.Automation.Language.ScriptExtent' -ArgumentList $innerExtentStartPos,$innerExtentEndPos

           [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]::Contains($outerExtent, $innerExtent) | Should Be $true
        }
    }
}
