$settings = @{
    IncludeRules = @('PSAvoidUsingDoubleQuotesForConstantString')
    Rules        = @{
        PSAvoidUsingDoubleQuotesForConstantString = @{
            Enable = $true
        }
    }
}

Describe 'AvoidUsingDoubleQuotesForConstantString' {
    Context 'One line string' {
        It 'Should warn if string is constant and double quotes are used' {
            (Invoke-ScriptAnalyzer -ScriptDefinition '$item = "value"' -Settings $settings).Count | Should -Be 1
        }

        It 'Should correctly format if string is constant and double quotes are used' {
            Invoke-Formatter -ScriptDefinition '$item = "value"' -Settings $settings | Should -Be "`$item = 'value'"
        }

        It 'Should not warn if string is interpolated and double quotes are used but single quotes are in value' {
            Invoke-ScriptAnalyzer -ScriptDefinition "`$item = 'value'" -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is interpolated and double quotes are used but backtick character is in value' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$item = "foo-`$-bar"' -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is constant and single quotes are used' {
            Invoke-ScriptAnalyzer -ScriptDefinition "`$item = 'value'" -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is interpolated and double quotes are used' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$item = "$(Get-Item)"' -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is interpolated and double quotes are used' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$item = "$(Get-Item)"' -Settings $settings | Should -BeNullOrEmpty
        }
    }

    # TODO: check escape strings

    Context 'Here string' {
        It 'Should warn if string is constant and double quotes are used' {
            $scriptDefinition = @'
$item=@"
value
"@
'@
            (Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings).Count | Should -Be 1
        }

        It 'Should correctly format if string is constant and double quotes are used' {
            $scriptDefinition = @'
$item=@"
value
"@
'@
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings $settings | Should -Be @"
`$item=@'
value
'@
"@
        }

        It 'Should not warn if string is constant and double quotes are used but @'' is used in value' {
            $scriptDefinition = @'
$item=@"
value1@'value2
"@
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings | Should -BeNullOrEmpty
        }
        It 'Should not warn if string is constant and double quotes are used but backtick is used in value' {
            $scriptDefinition = @'
$item=@"
foo-`$-bar
"@
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is constant and single quotes are used' {
            $scriptDefinition = @"
`$item=@'
value
'@
"@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is interpolated' {
            $scriptDefinition = @'
$item=@"
$(Get-Process)
"@
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings | Should -BeNullOrEmpty
        }
    }

}
