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

    It "Corrects alias correctly" {
        Invoke-Formatter 'Gci' | Should -Be 'gci'
        Invoke-Formatter '?' | Should -Be '?'
    }

    It "Corrects applications on Windows to not end in .exe" -Skip:($IsLinux -or $IsMacOS) {
        Invoke-Formatter 'Cmd' | Should -Be 'cmd'
    }

    It "corrects case of script function" {
        function Invoke-DummyFunction
        {

        }
        Invoke-Formatter 'invoke-dummyFunction' | Should -Be 'Invoke-DummyFunction'
    }
}