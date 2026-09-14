# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Analysis metadata work" {
    BeforeAll {
        $null = Invoke-ScriptAnalyzer -ScriptDefinition 'Write-Output example'
        $telemetry = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper].Assembly.GetType(
            'Microsoft.Windows.PowerShell.ScriptAnalyzer.PerformanceTelemetry')
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }
        $manifestRules = @('PSMissingModuleManifestField', 'PSUseToExportFieldsInManifest')
    }

    BeforeEach {
        $telemetry.GetMethod('Reset').Invoke($null, @())
        $telemetry.GetProperty('Enabled').SetValue($null, $true)
    }

    AfterEach {
        $telemetry.GetProperty('Enabled').SetValue($null, $false)
    }

    It "does not query metadata for commands with no recursive parameter ASTs" {
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition 'get-item; write-output example' -Settings $casingSettings)
        $diagnostics.Count | Should -Be 2
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataQueries'] | Should -Be 0
    }

    It "checks static, dynamic and alias parameter casing without a fresh-command fallback" {
        foreach ($iteration in 1..100) {
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition (
                'Write-Output -inputobject example; Get-Item -path .; gci -literalpath .'
            ) -Settings $casingSettings -ErrorAction Stop)
            $diagnostics.Count | Should -Be 3
            $diagnostics.SuggestedCorrections.Text | Should -Contain 'InputObject'
            $diagnostics.SuggestedCorrections.Text | Should -Contain 'Path'
            $diagnostics.SuggestedCorrections.Text | Should -Contain 'LiteralPath'
        }
        $stats = $telemetry.GetMethod('Snapshot').Invoke($null, @())
        $stats['LookupBypasses'] | Should -Be 0
        $stats['LookupResolutionFailures'] | Should -Be 0
        $stats['LookupRetries'] | Should -Be 0
        $stats['MetadataFailures'] | Should -Be 0
        $stats['MetadataRetries'] | Should -Be 0
    }

    It "preserves recursive casing checks for parameters nested in script blocks" {
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition 'Get-Item { Unknown-SnapshotCommand -path value }' -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataQueries'] | Should -Be 1
    }

    It "does not query export metadata when no export command matches" {
        $ast = [System.Management.Automation.Language.Parser]::ParseInput('function Test-Example {}', [ref]$null, [ref]$null)
        [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::Instance.GetExportedFunction($ast).Count | Should -Be 0
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataQueries'] | Should -Be 0
    }

    It "preserves mandatory parameter matching for <Script>" -TestCases @(
        @{ Script = 'Write-Warning'; Count = 1 }
        @{ Script = 'Write-Warning -Message example'; Count = 0 }
        @{ Script = 'Write-Warning -Mes example'; Count = 1 }
        @{ Script = 'Write-Warning example'; Count = 0 }
        @{ Script = 'Write-Warning @parameters'; Count = 0 }
        @{ Script = '"example" | Write-Warning'; Count = 0 }
    ) {
        param($Script, $Count)
        @(Invoke-ScriptAnalyzer -ScriptDefinition $Script -IncludeRule PSUseCmdletCorrectly).Count | Should -Be $Count
    }

    It "keeps PackageManagement on its metadata-free special path" {
        if ($null -eq [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::Instance.GetCommandInfo('Install-Package')) {
            Set-ItResult -Skipped -Because 'PackageManagement is not installed'
            return
        }
        @(Invoke-ScriptAnalyzer -ScriptDefinition 'Install-Package' -IncludeRule PSUseCmdletCorrectly).Count | Should -Be 1
        @(Invoke-ScriptAnalyzer -ScriptDefinition 'Install-Package -Na example' -IncludeRule PSUseCmdletCorrectly).Count | Should -Be 0
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['MetadataQueries'] | Should -Be 0
    }

    It "shares manifest results and errors only within one analysis" {
        $manifest = Join-Path $TestDrive 'Freshness.psd1'
        Set-Content $manifest "@{ ModuleVersion = '1.0'; FunctionsToExport = '*' }"
        $diagnostics = @(Invoke-ScriptAnalyzer -Path $manifest -IncludeRule $manifestRules -ErrorAction Stop)
        $diagnostics.RuleName | Should -Contain 'PSUseToExportFieldsInManifest'
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['ManifestValidations'] | Should -Be 1

        Set-Content $manifest "@{ FunctionsToExport = '*' }"
        $diagnostics = @(Invoke-ScriptAnalyzer -Path $manifest -IncludeRule $manifestRules -ErrorAction Stop)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].RuleName | Should -Be 'PSMissingModuleManifestField'
        $diagnostics[0].Message | Should -Match 'ModuleVersion'
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['ManifestValidations'] | Should -Be 2

        Set-Content $manifest "@{ ModuleVersion = '2.0'; FunctionsToExport = @(); CmdletsToExport = @(); AliasesToExport = @() }"
        @(Invoke-ScriptAnalyzer -Path $manifest -IncludeRule $manifestRules -ErrorAction Stop).Count | Should -Be 0
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['ManifestValidations'] | Should -Be 3
    }

    It "does not change public manifest lookup freshness outside an analysis" {
        $manifest = Join-Path $TestDrive 'PublicLookup.psd1'
        Set-Content $manifest "@{ ModuleVersion = '1.0' }"
        $errors = $null
        $helper = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::Instance
        $helper.GetModuleManifest($manifest, [ref]$errors).Version | Should -Be ([version]'1.0')
        Set-Content $manifest "@{ ModuleVersion = '2.0' }"
        $helper.GetModuleManifest($manifest, [ref]$errors).Version | Should -Be ([version]'2.0')
        $helper.GetModuleManifestForAnalysis($manifest, [ref]$errors).Version | Should -Be ([version]'2.0')
        $telemetry.GetMethod('Snapshot').Invoke($null, @())['ManifestValidations'] | Should -Be 3
    }
}
