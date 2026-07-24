Describe "Validate rule documentation files" {
    BeforeAll {
        $ruleDocDirectory = 'C:\Git\PS-Src\PSScriptAnalyzer\docs\Rules' #Join-Path $PSScriptRoot '../../docs/Rules'
        $docInfoList = @{}
        Get-ChildItem $ruleDocDirectory/*.md -Exclude README.md |
            ForEach-Object {
                $sev = Select-String -Path $_ -Pattern '\*\*Severity Level: (?<sev>\w+)\*\*'
                $def = Select-String -Path $_ -Pattern '\*\*Default state: (?<def>\w+\s?\w+)\*\*'
                $docInfoList.Add(('PS' + $_.BaseName), [pscustomobject]@{
                    FileName = $_.Name
                    Severity = $sev.Matches.Groups.Where({$_.Name -eq 'sev'}).Value
                    DefState = $def.Matches.Groups.Where({$_.Name -eq 'def'}).Value
                })
            }
        #$docInfoList

        $ruleList = Get-ScriptAnalyzerRule |
            Sort-Object RuleName |
            Select-Object -Property RuleName, Severity
        #$ruleList

        $ruleTable = @()
        $linkDefs = @{}
        $linkDefLine = @{}
        $usedRefs = @{}
        # Regex patterns.
        # Table rule cell: | [RuleName][ref] | Severity | Default state |... |  -> capture name, ref, severity, defstate.
        $ruleRowRegex = '^\|\s*\[(?<name>[^\]]+)\]\[(?<ref>[^\]]+)\]\s*\|\s*(?<severity>[^|]+?)\s*\|(?<defstate>[^|]+?)\s*\|'
        # Link definition: [ref]: target  (target may include an #anchor).
        $linkDefRegex = '^\[(?<ref>[^\]]+)\]:\s*(?<target>\S+)'
        # Any reference-style usage anywhere: ...][ref]...
        $refUsageRegex = '\]\[(?<ref>[^\]]+)\]'
        $lines = Get-Content 'C:\Git\PS-Src\PSScriptAnalyzer\docs\Rules\README.md'
        $lineNumber = 0
        foreach ($line in $lines) {
            $lineNumber++
            if ($line -match $ruleRowRegex) {
                $ruleTable += [pscustomobject]@{
                    RowName  = $Matches['name'].Trim()
                    RuleName = 'PS' + $Matches['name'].Trim()
                    Ref      = $Matches['ref'].Trim()
                    Severity = $Matches['severity'].Trim()
                    DefState = $Matches['defstate'].Trim()
                    Line     = $lineNumber
                }
            }

            if ($line -match $linkDefRegex) {
                $ref = $Matches['ref'].Trim()
                $linkDefs[$ref] = $Matches['target'].Trim()
                $linkDefLine[$ref] = $lineNumber
            }

            # Collect every reference usage (table rows and prose) for orphan detection.
            foreach ($match in [regex]::matches($line, $refUsageRegex)) {
                $usedRefs[$match.Groups['ref'].Value] = 1
            }
        }
    }

    #########################################################################################

    It 'Every rule documentation file must be a defined rule' {
        $result = $true
        foreach ($rule in $docInfoList.Keys) {
            if ($rule -notin $ruleList.RuleName) {
                Write-Host "Rule not defined for file: $($docInfoList[$rule].FileName)"
                $result = $false
            }
        }
        $result | Should -Be $true
    }

    It 'Every defined rule must have a documentation file' {
        $result = $true
        foreach ($rule in $ruleList.RuleName) {
            if ($rule -notin $docInfoList.Keys) {
                Write-Host "Missing documentation file for rule: $($rule)"
                $result = $false
            }
        }
        $result | Should -Be $true
    }

    It 'Every rule doc must have the correct defined severity' {
        $result = $true
        foreach ($rule in $ruleList) {
            if ($rule.Severity -ne $docInfoList[$rule.RuleName].Severity) {
                Write-Host "Severity mismatch for rule: $($rule.RuleName). Defined: $($rule.Severity), Doc: $($docInfoList[$rule.RuleName].Severity)"
                $result = $false
            }
        }
        $result | Should -Be $true
    }

    It 'Every rule in the table must have the correct severity and default state' {
        $result = $true
        foreach ($ruleRow in $ruleTable) {
            $definedRule = $ruleList | Where-Object { $_.RuleName -eq $ruleRow.RuleName }
            if ($null -eq $definedRule) {
                Write-Host "Rule in table not found in defined rules: $($ruleRow.RowName)"
                $result = $false
                continue
            }
            if ($ruleRow.RuleName -notin $docInfoList.Keys) {
                Write-Host "Rule in table not found in documentation: $($ruleRow.RowName)"
                $result = $false
                continue
            }
            $docInfo = $docInfoList[$ruleRow.RuleName]
            if ($ruleRow.Severity -ne $docInfo.Severity) {
                Write-Host "Severity mismatch for rule: $($ruleRow.RowName). Table: $($ruleRow.Severity), Doc: $($docInfo.Severity)"
                $result = $false
            }
            if ($ruleRow.DefState -ne $docInfo.DefState) {
                Write-Host "Default state mismatch for rule: $($ruleRow.RowName). Table: $($ruleRow.DefState), Doc: $($docInfo.DefState)"
                $result = $false
            }
        }
        $result | Should -Be $true
    }

    It 'Every link definition must be used at least once' {
        $result = $true
        foreach ($ref in $linkDefs.Keys) {
            if ($ref -notin $usedRefs.Keys) {
                Write-Host "Unused link definition: $ref (defined at line $($linkDefLine[$ref]))"
                $result = $false
            }
        }
        $result | Should -Be $true
    }

    It 'Every link definition that points to a rule must have a matching rule in the table' {
        $result = $true
        foreach ($ref in $linkDefs.Keys) {
            $target = $linkDefs[$ref]
            $isRuleFile = $targetFile -match '\.md$' -and $targetFile -notmatch '/'

            if ($isRuleFile) {
                # A rule-page link definition with no matching table row.
                if ($ref -notin $ruleTable.Ref) {
                    Write-Host "Orphan rule target: Link definition [$ref] -> '$target' has no matching rule in the table (defined at line $($linkDefLine[$ref]))."
                    $result = $false
                }
            }
        }
        $result | Should -Be $true
    }

    It 'Every link definition target must have a valid file path' {
        $result = $true
        foreach ($ref in $linkDefs.Keys) {
            $target = $linkDefs[$ref]
            $isRuleFile = $targetFile -match '\.md$' -and $targetFile -notmatch '/'

            if ($isRuleFile) {
                # A rule-page link definition with no matching table row.
                if ($target -notin $ruleTable.FileName) {
                    Write-Host "Orphan rule target: Link definition [$ref] -> '$target' has no matching rule file."
                    $result = $false
                }
            }
        }
        $result | Should -Be $true
    }
}
