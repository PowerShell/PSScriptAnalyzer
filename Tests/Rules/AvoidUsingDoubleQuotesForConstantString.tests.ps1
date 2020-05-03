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

        It 'Should not warn if string is interpolated and double quotes are used but single quotes are in value' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$item = "''value''"' -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is constant and signle quotes are used' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$item = ''value''' -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not warn if string is interpolated and double quotes are used' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$item = "$(Get-Item)"' -Settings $settings | Should -BeNullOrEmpty
        }
    }

    Context 'Here string' {
        It 'Should warn if string is constant and double quotes are used' {
            $scriptDefinition = @'
$item=@"
value
"@
'@
            (Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings).Count | Should -Be 1
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
