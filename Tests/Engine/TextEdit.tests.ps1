Describe "TextEdit Class" {
    BeforeAll {
        $type = [Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit]
    }

    Context "Object construction" {
        It "creates the object with correct properties" {
            $correctionExtent = New-Object -TypeName $type -ArgumentList 1, 2, 3, 4, "get-childitem"
            $correctionExtent.StartLineNumber | Should -Be 1
            $correctionExtent.EndLineNumber | Should -Be 3
            $correctionExtent.StartColumnNumber | Should -Be 2
            $correctionExtent.EndColumnNumber | Should -Be 4
            $correctionExtent.Text | Should -Be "get-childitem"
        }

        It "throws if end line number is less than start line number" {
            $text = "Get-ChildItem"
            { New-Object -TypeName $type -ArgumentList @(2, 1, 1, ($text.Length + 1), $text) } |
                Should -Throw
        }

        It "throws if end column number is less than start column number for same line" {
            $text = "start-process"
            { New-Object -TypeName $type -ArgumentList @(1, 2, 1, 1, $text) } |
                Should -Throw
        }
    }
}
