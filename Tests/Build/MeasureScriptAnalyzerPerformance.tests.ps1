# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'Performance benchmark diagnostic normalization' -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
    BeforeAll {
        $harnessPath = Join-Path $PSScriptRoot '../../tools/Measure-ScriptAnalyzerPerformance.ps1'
        $tokens = $null
        $parseErrors = $null
        $harness = [System.Management.Automation.Language.Parser]::ParseFile(
            $harnessPath, [ref]$tokens, [ref]$parseErrors)
        if ($parseErrors.Count) { throw $parseErrors[0] }
        foreach ($name in 'ConvertTo-WorkloadPath', 'ConvertTo-DiagnosticJson') {
            $function = $harness.Find({
                param($node)
                $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name
            }, $false)
            . ([scriptblock]::Create($function.Extent.Text))
        }

        function Get-TestDiagnostic {
            param([string]$Root)
            $file = Join-Path $Root 'example.ps1'
            [pscustomobject]@{
                RuleName = 'PSUseCorrectCasing'
                Severity = 'Information'
                Message = "Incorrect casing in $Root"
                ScriptName = 'example.ps1'
                ScriptPath = $file
                RuleSuppressionID = 'Get-Item'
                IsSuppressed = $false
                Extent = [pscustomobject]@{
                    File = $file
                    StartLineNumber = 1
                    StartColumnNumber = 1
                    EndLineNumber = 1
                    EndColumnNumber = 9
                    StartOffset = 0
                    EndOffset = 8
                    Text = 'get-item'
                }
                SuggestedCorrections = @([pscustomobject]@{
                    File = $file
                    StartLineNumber = 1
                    StartColumnNumber = 1
                    EndLineNumber = 1
                    EndColumnNumber = 9
                    Text = 'Get-Item'
                    Description = 'Correct command casing'
                })
            }
        }
    }

    It 'ignores checkout-root differences while preserving diagnostic content' {
        $inputRoot = Join-Path $TestDrive 'first'
        $first = ConvertTo-DiagnosticJson (Get-TestDiagnostic $inputRoot)
        $inputRoot = Join-Path $TestDrive 'second'
        $second = ConvertTo-DiagnosticJson (Get-TestDiagnostic $inputRoot)
        $first | Should -BeExactly $second
        ($first | ConvertFrom-Json).ScriptPath | Should -BeExactly 'example.ps1'
    }

    It 'detects changes in <Field> even when diagnostic counts match' -ForEach @(
        @{ Field = 'Message' }
        @{ Field = 'RuleName' }
        @{ Field = 'RuleSuppressionID' }
    ) {
        $inputRoot = $TestDrive
        $diagnostic = Get-TestDiagnostic $inputRoot
        $before = ConvertTo-DiagnosticJson $diagnostic
        $diagnostic.$Field = 'changed'
        ConvertTo-DiagnosticJson $diagnostic | Should -Not -BeExactly $before
    }

    It 'preserves locations, suppression state and suggested corrections' {
        $inputRoot = $TestDrive
        $diagnostic = Get-TestDiagnostic $inputRoot
        $before = ConvertTo-DiagnosticJson $diagnostic
        $diagnostic.Extent.StartOffset = 1
        ConvertTo-DiagnosticJson $diagnostic | Should -Not -BeExactly $before
        $diagnostic.Extent.StartOffset = 0
        $diagnostic.IsSuppressed = $true
        ConvertTo-DiagnosticJson $diagnostic | Should -Not -BeExactly $before
        $diagnostic.IsSuppressed = $false
        $diagnostic.SuggestedCorrections[0].Text = 'Different-Command'
        ConvertTo-DiagnosticJson $diagnostic | Should -Not -BeExactly $before
    }

    It 'handles missing extents and corrections' {
        $inputRoot = $TestDrive
        $diagnostic = Get-TestDiagnostic $inputRoot
        $diagnostic.Extent = $null
        $diagnostic.SuggestedCorrections = $null
        $result = ConvertTo-DiagnosticJson $diagnostic | ConvertFrom-Json
        $result.Extent | Should -BeNullOrEmpty
        $result.SuggestedCorrections.Count | Should -Be 0
    }
}

Describe 'Performance comparison metrics policy' -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
    BeforeAll {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../.github/workflows/performance.yml') -Raw
        $comparison = [regex]::Match($workflow, '(?ms)      - name: Validate and summarize\r?\n.*?        run: \|\r?\n(?<script>.*?)(?=      - name:)')
        if (-not $comparison.Success) { throw 'Cannot find performance comparison script.' }
        $comparisonScript = [scriptblock]::Create(
            [regex]::Replace($comparison.Groups['script'].Value, '(?m)^          ', ''))
    }

    BeforeEach {
        $savedWorkspace = $env:GITHUB_WORKSPACE
        $savedRepetitions = $env:BENCHMARK_REPETITIONS
        $savedSummary = $env:GITHUB_STEP_SUMMARY
        $env:GITHUB_WORKSPACE = $TestDrive
        $env:BENCHMARK_REPETITIONS = '1'
        $env:GITHUB_STEP_SUMMARY = Join-Path $TestDrive 'summary.txt'
        $resultDirectory = Join-Path $TestDrive 'results'
        $null = New-Item -ItemType Directory -Path $resultDirectory -Force
        foreach ($build in 'upstream', 'fork', 'perf') {
            @{
                Revision = $build
                MetricsRequested = $false
                MetricsAvailable = $build -eq 'perf'
                ColdDiagnosticCount = 1
                WarmDiagnosticCount = 1
                ColdDiagnosticsSHA256 = 'identical'
                WarmDiagnosticsSHA256 = 'identical'
                ColdSeconds = 2.0
                WarmSeconds = 1.0
            } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $resultDirectory "$build-1.json")
        }
    }

    AfterEach {
        $env:GITHUB_WORKSPACE = $savedWorkspace
        $env:BENCHMARK_REPETITIONS = $savedRepetitions
        $env:GITHUB_STEP_SUMMARY = $savedSummary
    }

    It 'does not offer or enable metrics collection' {
        $workflow | Should -Not -Match 'collect_metrics|BENCHMARK_METRICS|-CollectMetrics'
    }

    It 'compares uninstrumented samples even when only perf supports metrics' {
        { & $comparisonScript } | Should -Not -Throw
        $report = Get-Content -LiteralPath (Join-Path $resultDirectory 'comparison.json') -Raw | ConvertFrom-Json
        $report.FindingsVerified | Should -BeTrue
        $report.Measurements.Count | Should -Be 3
    }

    It 'rejects metrics-enabled <Source> samples' -ForEach @(
        @{ Source = 'upstream' }
        @{ Source = 'fork' }
        @{ Source = 'perf' }
    ) {
        $path = Join-Path $resultDirectory "$Source-1.json"
        $sample = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
        $sample.MetricsRequested = $true
        $sample | ConvertTo-Json | Set-Content -LiteralPath $path
        { & $comparisonScript } | Should -Throw '*Metrics must be disabled for performance comparisons.*'
    }
}
