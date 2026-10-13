# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'Parallel rule execution' {
    It 'analyzes many files with most rules enabled without failing' {
        # Regression test for https://github.com/PowerShell/PSScriptAnalyzer/issues/2205:
        # rules run in parallel for each file and share command lookups and metadata queries,
        # which intermittently failed because the PowerShell engine is not thread safe.
        # The workload is synthetic so the test stays fast and portable, while the aliases,
        # miscased parameters, mandatory parameters and exports below still force every
        # analyzed file through concurrent command lookups and metadata queries.
        $settingsPath = Join-Path $PSScriptRoot 'ParallelRuleExecution/PSScriptAnalyzerSettings.psd1'
        $root = Join-Path $TestDrive 'ParallelRuleExecution'
        $null = New-Item -ItemType Directory -Path $root -Force
        foreach ($index in 1..20) {
            @"
function Test-ParallelExample$index {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]`$Path)
    gci -LiteralPath `$Path | where Name -like '*.ps1' | foreach {
        Write-Output -inputobject `$_.FullName
        Get-Item -path `$_.FullName
        Test-Path -LiteralPath `$_.FullName
    }
}
Export-ModuleMember -Function Test-ParallelExample$index
"@ | Set-Content -LiteralPath (Join-Path $root "Example$index.psm1")
        }

        $diagnostics = Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $settingsPath -ErrorAction Stop
        @($diagnostics).Count | Should -BeGreaterThan 0
    }
}
