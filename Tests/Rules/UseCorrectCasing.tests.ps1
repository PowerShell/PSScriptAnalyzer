Describe "UseCorrectCasing" {
    It "corrects case of simple cmdlet" {
        Invoke-Formatter 'get-childitem' | Should -Be 'Get-ChildItem'
    }

    It "corrects case of fully qualified cmdlet" {
        Invoke-Formatter 'Microsoft.PowerShell.management\get-childitem' | Should -Be 'Microsoft.PowerShell.Management\Get-ChildItem'
    }

    It "corrects case of of cmdlet inside interpolated string" {
        Invoke-Formatter '"$(get-childitem)"' | Should -Be '"$(get-childitem)"'
    }

    It "corrects case of script function" {
        function Invoke-DummyFunction
        {

        }
        Invoke-Formatter 'invoke-dummyFunction' | Should -Be 'Invoke-DummyFunction'
        Invoke-ScriptAnalyzer 'foo' # Workaround for this bug@ https://github.com/PowerShell/PSScriptAnalyzer/issues/1116
    }
}