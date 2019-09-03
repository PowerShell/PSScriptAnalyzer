$ruleName = "PSAvoidLongLines"

$ruleSettings = @{
    Enable = $true
}
$settings = @{
    IncludeRules = @($ruleName)
    Rules = @{ $ruleName = $ruleSettings }
}

Describe "AvoidLongLines" {
    it 'Should be off by default' {
        $def = "a" * 500
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def
        $violations.Count | Should -Be 0
    }

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

    it 'Should find a violation with a configured line length' {
        $ruleSettings.Add('LineLength', 10)
        $settings['Rules'] = @{ $ruleName = $ruleSettings }
        $def = "a" * 15
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations.Count | Should -Be 1
    }
}
