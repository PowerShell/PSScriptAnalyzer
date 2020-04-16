Describe "Validate rule documentation files" {
    BeforeAll {
        $ruleDocDirectory = Join-Path $PSScriptRoot '../../RuleDocumentation'
        $docs = Get-ChildItem $ruleDocDirectory/*.md -Exclude README.md |
            ForEach-Object { "PS" + $_.BaseName} | Sort-Object

        $rules = Get-ScriptAnalyzerRule | ForEach-Object RuleName | Sort-Object

        $readmeLinks = @{}
        $readmeRules = Get-Content -LiteralPath $ruleDocDirectory/README.md |
            Foreach-Object { if ($_ -match '^\s*\|\s*\[([^]]+)\]\(([^)]+)\)\s*\|') {
                $ruleName = $matches[1] -replace '<sup>.</sup>$', ''
                $readmeLinks["$ruleName"] = $matches[2]
                "PS${ruleName}"
            }} |
            Sort-Object

        # Remove rules from the diff list that aren't supported on PSCore
        if (($PSVersionTable.PSVersion.Major -ge 6) -and ($PSVersionTable.PSEdition -eq "Core"))
        {
            $RulesNotSupportedInNetstandard2 = @("PSUseSingularNouns")
            $docs = $docs | Where-Object {$RulesNotSupportedInNetstandard2 -notcontains $_}
            $readmeRules = $readmeRules | Where-Object { $RulesNotSupportedInNetstandard2 -notcontains $_ }
        }
        elseif ($PSVersionTable.PSVersion.Major -eq 4) {
            $docs = $docs | Where-Object {$_ -notmatch '^PSAvoidGlobalAliases$'}
            $readmeRules = $readmeRules | Where-Object { $_ -notmatch '^PSAvoidGlobalAliases$' }
        }

        $rulesDocsDiff = Compare-Object -ReferenceObject $rules -DifferenceObject $docs -SyncWindow 25
        $rulesReadmeDiff = Compare-Object -ReferenceObject $rules -DifferenceObject $readmeRules -SyncWindow 25
    }

    It "Every rule must have a rule documentation file" {
        $rulesDocsDiff | Where-Object SideIndicator -eq "<=" | Foreach-Object InputObject | Should -BeNullOrEmpty
    }
    It "Every rule documentation file must have a corresponding rule" {
        $rulesDocsDiff | Where-Object SideIndicator -eq "=>" | Foreach-Object InputObject | Should -BeNullOrEmpty
    }

    It "Every rule must have an entry in the rule documentation README.md file" {
        $rulesReadmeDiff | Where-Object SideIndicator -eq "<=" | Foreach-Object InputObject | Should -BeNullOrEmpty
    }
    It "Every entry in the rule documentation README.md file must correspond to a rule" {
        $rulesReadmeDiff | Where-Object SideIndicator -eq "=>" | Foreach-Object InputObject | Should -BeNullOrEmpty
    }

    It "Every entry in the rule documentation README.md file must have a valid link to the documentation file" {
        foreach ($key in $readmeLinks.Keys) {
            $link = $readmeLinks[$key]
            $filePath = Join-Path $ruleDocDirectory $link
            $filePath | Should -Exist
        }
    }

    It "Every rule name in the rule documentation README.md file must match the documentation file's basename" {
        foreach ($key in $readmeLinks.Keys) {
            $link = $readmeLinks[$key]
            $filePath = Join-Path $ruleDocDirectory $link
            $fileName = Split-Path $filePath -Leaf
            $fileName | Should -BeExactly "${key}.md"
        }
    }
}
