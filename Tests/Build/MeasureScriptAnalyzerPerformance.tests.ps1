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

Describe 'Performance benchmark build matrix' -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
    BeforeAll {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../.github/workflows/performance.yml') -Raw
        $matrix = [regex]::Match($workflow, '(?ms)^  benchmark:.*?      matrix:\r?\n(?<matrix>.*?)^    defaults:').Groups['matrix'].Value
        $builds = [regex]::Matches($workflow, '(?m)^        run: ''(?<command>& .*?/analyzer/build\.ps1.*?)''\r?$')
        $builds.Count | Should -Be 2
        $null = New-Item -ItemType Directory -Path (Join-Path $TestDrive 'analyzer') -Force
        Set-Content -LiteralPath (Join-Path $TestDrive 'analyzer/build.ps1') -Value @'
[CmdletBinding()]
param([string]$Configuration, [int]$PSVersion, [switch]$DisableEngineRetries)
[pscustomobject]@{
    Configuration = $Configuration
    PSVersion = $PSVersion
    RetrySwitchBound = $PSBoundParameters.ContainsKey('DisableEngineRetries')
    DisableEngineRetries = $DisableEngineRetries.IsPresent
}
'@
    }

    BeforeEach {
        $savedWorkspace = $env:GITHUB_WORKSPACE
        $env:GITHUB_WORKSPACE = $TestDrive
    }

    AfterEach {
        $env:GITHUB_WORKSPACE = $savedWorkspace
    }

    It 'crosses all four benchmark sources with both operating systems and workloads' {
        $matrix | Should -Match '(?m)^        os: \[ubuntu-latest, windows-latest\]'
        $matrix | Should -Match '(?m)^        workload: \[powershell, semver\]'
        $matrix | Should -Match '(?m)^        benchmark-source: \[upstream, fork, perf, perf-no-retry\]'
    }

    It 'maps <Benchmark> to the correct revision and retry setting' -ForEach @(
        @{ Benchmark = 'upstream'; Source = 'upstream'; Repository = 'PowerShell/PSScriptAnalyzer'; Disabled = 'false' }
        @{ Benchmark = 'fork'; Source = 'fork'; Repository = 'jessehouwing/PSScriptAnalyzer'; Disabled = 'false' }
        @{ Benchmark = 'perf'; Source = 'perf'; Repository = 'jessehouwing/PSScriptAnalyzer'; Disabled = 'false' }
        @{ Benchmark = 'perf-no-retry'; Source = 'perf'; Repository = 'jessehouwing/PSScriptAnalyzer'; Disabled = 'true' }
    ) {
        $matrix | Should -Match "(?m)^          - source: $Source\r?\n            benchmark-source: $Benchmark\r?\n            repository: $Repository\r?\n            disable-engine-retries: $Disabled\r?$"
    }

    It 'explicitly binds DisableEngineRetries to <Disabled> for perf builds' -ForEach @(
        @{ Disabled = 'false'; Expected = $false }
        @{ Disabled = 'true'; Expected = $true }
    ) {
        $command = $builds[1].Groups['command'].Value.Replace('${{ matrix.disable-engine-retries }}', $Disabled)
        $result = & ([scriptblock]::Create($command))
        $result.RetrySwitchBound | Should -BeTrue
        $result.DisableEngineRetries | Should -Be $Expected
        $result.Configuration | Should -Be 'Release'
        $result.PSVersion | Should -Be 7
        $workflow | Should -Match "(?m)^        if: matrix.source == 'perf'\r?$"
    }

    It 'does not pass the perf-only retry switch to baseline builds' {
        $result = & ([scriptblock]::Create($builds[0].Groups['command'].Value))
        $result.RetrySwitchBound | Should -BeFalse
        $workflow | Should -Match "(?m)^        if: matrix.source != 'perf'\r?$"
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
        foreach ($build in 'upstream', 'fork', 'perf', 'perf-no-retry') {
            @{
                Revision = $build
                DisableEngineRetries = $build -eq 'perf-no-retry'
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
        $report.Measurements.Count | Should -Be 4
        $text = Get-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Raw
        $text | Should -Match '\| perf \(.*\| False \|'
        $text | Should -Match '\| perf-no-retry \(.*\| True \|'
    }

    It 'rejects metrics-enabled <Source> samples' -ForEach @(
        @{ Source = 'upstream' }
        @{ Source = 'fork' }
        @{ Source = 'perf' }
        @{ Source = 'perf-no-retry' }
    ) {
        $path = Join-Path $resultDirectory "$Source-1.json"
        $sample = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
        $sample.MetricsRequested = $true
        $sample | ConvertTo-Json | Set-Content -LiteralPath $path
        { & $comparisonScript } | Should -Throw '*Metrics must be disabled for performance comparisons.*'
    }
}

Describe 'Consolidated performance summary' -Skip:($PSVersionTable.PSVersion.Major -lt 7) {
    BeforeAll {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../../.github/workflows/performance.yml') -Raw
        $summary = [regex]::Match($workflow, '(?ms)      - name: Summarize all combinations\r?\n.*?        run: \|\r?\n(?<script>.*)\z')
        if (-not $summary.Success) { throw 'Cannot find consolidated summary script.' }
        $summaryScript = [scriptblock]::Create(
            [regex]::Replace($summary.Groups['script'].Value, '(?m)^          ', ''))
    }

    BeforeEach {
        $savedWorkspace = $env:GITHUB_WORKSPACE
        $savedSummary = $env:GITHUB_STEP_SUMMARY
        $env:GITHUB_WORKSPACE = $TestDrive
        $env:GITHUB_STEP_SUMMARY = Join-Path $TestDrive 'summary.txt'
        Set-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value ''
        foreach ($os in 'ubuntu-latest', 'windows-latest') {
            foreach ($workload in 'powershell', 'semver') {
                $directory = Join-Path $TestDrive "comparisons/scriptanalyzer-performance-$os-$workload-comparison"
                $null = New-Item -ItemType Directory -Path $directory -Force
                @{
                    FindingsVerified = $true
                    Measurements = @(foreach ($source in 'upstream', 'fork', 'perf', 'perf-no-retry') {
                        foreach ($seconds in 9.0, 1.0, 2.0) {
                            @{
                                Source = $source
                                ColdSeconds = $seconds
                                WarmSeconds = $seconds / 2
                                Inconsistent = $false
                            }
                        }
                    })
                } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $directory 'comparison.json')
            }
        }
        $comparisonPath = Join-Path $TestDrive 'comparisons/scriptanalyzer-performance-ubuntu-latest-powershell-comparison/comparison.json'
    }

    AfterEach {
        $env:GITHUB_WORKSPACE = $savedWorkspace
        $env:GITHUB_STEP_SUMMARY = $savedSummary
    }

    It 'shows all sixteen combinations with median rather than mean timings' {
        & $summaryScript
        $text = Get-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Raw
        [regex]::Matches($text, '(?m)^\| (ubuntu|windows)-latest \|').Count | Should -Be 16
        foreach ($os in 'ubuntu-latest', 'windows-latest') {
            foreach ($workload in 'powershell', 'semver') {
                foreach ($source in 'upstream', 'fork', 'perf', 'perf-no-retry') {
                    $text | Should -Match ([regex]::Escape("| $os | $workload | $source | 2.000 | 1.000 | Verified |"))
                }
            }
        }
    }

    It 'marks missing combinations without dropping the other results' {
        Remove-Item -LiteralPath $comparisonPath
        & $summaryScript
        $text = Get-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Raw
        [regex]::Matches($text, '\| Missing results \|').Count | Should -Be 4
        [regex]::Matches($text, '\| Verified \|').Count | Should -Be 12
    }

    It 'still produces the full table when no comparisons are available' {
        Remove-Item -LiteralPath (Join-Path $TestDrive 'comparisons') -Recurse
        & $summaryScript
        $text = Get-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Raw
        [regex]::Matches($text, '\| Missing results \|').Count | Should -Be 16
    }

    It 'marks invalid comparisons and retried samples' {
        $report = Get-Content -LiteralPath $comparisonPath -Raw | ConvertFrom-Json
        $report.FindingsVerified = $false
        $report.Measurements[0].Inconsistent = $true
        $report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $comparisonPath
        & $summaryScript
        $text = Get-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Raw
        [regex]::Matches($text, 'INVALID: diagnostics differ').Count | Should -Be 4
        $text | Should -Match 'upstream \| 2.000 \| 1.000 \| INVALID: diagnostics differ; ⚠️ retried samples'
    }
}
