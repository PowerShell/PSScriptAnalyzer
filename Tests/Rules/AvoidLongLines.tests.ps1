$ruleName = "PSAvoidLongLines"

$settings = @{
    IncludeRules = @($ruleName)
}

Describe "AvoidLongLines" {
    it 'Should find a violation when a line is longer than 120 characters (no whitespace)' {
        $def = "a" * 125
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations.Count | Should -Be 1
    }

    it 'Should find a violation when a line is longer than 120 characters (leading whitespace)' {
        $def = " " * 100 + "a" * 25
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations.Count | Should -Be 1
    }

    it 'Should not find a violation for lines under 120 characters' {
        $def = "a" * 120
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations.Count | Should -Be 0
    }
}
