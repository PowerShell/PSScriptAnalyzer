# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $sa = Get-Command Invoke-ScriptAnalyzer
    $singularNouns = "PSUseSingularNouns"
    $approvedVerb = "PSUseApprovedVerbs"
    $rules = Get-ScriptAnalyzerRule -Name ($singularNouns, "PSUseApprovedVerbs")
    $avoidRules = Get-ScriptAnalyzerRule -Name "PSAvoid*"
    $useRules = "PSUse*"
}

Describe "Test available parameters" {
    BeforeAll {
        $params = $sa.Parameters
    }

    Context "Path parameter" {
        It "has a Path parameter" {
            $params.ContainsKey("Path") | Should -BeTrue
        }

        It "accepts string" {
            $params["Path"].ParameterType.FullName | Should -Be "System.String"
        }
    }

    Context "Path parameter" {
        It "has a ScriptDefinition parameter" {
            $params.ContainsKey("ScriptDefinition") | Should -BeTrue
        }

        It "accepts string" {
            $params["ScriptDefinition"].ParameterType.FullName | Should -Be "System.String"
        }
    }

    Context "CustomRulePath parameters" {
        It "has a CustomRulePath parameter" {
            $params.ContainsKey("CustomRulePath") | Should -BeTrue
        }

        It "accepts a string array" {
            $params["CustomRulePath"].ParameterType.FullName | Should -Be "System.String[]"
        }

        It "has a CustomizedRulePath alias" {
            $params.CustomRulePath.Aliases.Contains("CustomizedRulePath") | Should -BeTrue
        }
    }

    Context "IncludeRule parameters" {
        It "has an IncludeRule parameter" {
            $params.ContainsKey("IncludeRule") | Should -BeTrue
        }

        It "accepts string array" {
            $params["IncludeRule"].ParameterType.FullName | Should -Be "System.String[]"
        }
    }

    Context "Severity parameters" {
        It "has a severity parameters" {
            $params.ContainsKey("Severity") | Should -BeTrue
        }

        It "accepts string array" {
            $params["Severity"].ParameterType.FullName | Should -Be "System.String[]"
        }
    }

    Context "SaveDscDependency parameter" -Skip:($testingLibraryUsage -or ($PSVersionTable.PSVersion -lt '5.0')) {
        It "has the parameter" {
            $params.ContainsKey("SaveDscDependency") | Should -BeTrue
        }

        It "is a switch parameter" {
            $params["SaveDscDependency"].ParameterType.FullName | Should -Be "System.Management.Automation.SwitchParameter"
        }

        It 'does not throw when being applied against a dummy script with no DSC code' {
            Invoke-ScriptAnalyzer -ScriptDefinition 'foo' -SaveDscDependency
        }
    }

    Context "It has 2 parameter sets: File and ScriptDefinition" {
        It "Has 2 parameter sets" {
            $sa.ParameterSets.Count | Should -Be 2
        }

        It "Has File parameter set" {
            $hasFile = $false
            foreach ($paramSet in $sa.ParameterSets) {
                if ($paramSet.Name -eq "File") {
                    $hasFile = $true
                    break
                }
            }

            $hasFile | Should -BeTrue
        }

        It "Has ScriptDefinition parameter set" {
            $hasFile = $false
            foreach ($paramSet in $sa.ParameterSets) {
                if ($paramSet.Name -eq "ScriptDefinition") {
                    $hasFile = $true
                    break
                }
            }

            $hasFile | Should -BeTrue
        }

    }
}

Describe "Test ScriptDefinition" {
    Context "When given a script definition" {
        It "Runs rules on script with more than 10 parser errors" {
            # this is a script with 12 parse errors
            $script = ');' * 12
            $moreThanTenErrors = Invoke-ScriptAnalyzer -ScriptDefinition $script
            $moreThanTenErrors.Count | Should -Be 12
        }
    }
}

Describe "Test Path" {
    Context "When given a single file" {
        It "Has the same effect as without Path parameter" {
            $scriptPath = Join-Path $PSScriptRoot "TestScript.ps1"
            $withPath = Invoke-ScriptAnalyzer $scriptPath
            $withoutPath = Invoke-ScriptAnalyzer -Path $scriptPath
            $withPath.Count | Should -Be $withoutPath.Count
        }
    }

    Context "When there are more than 10 errors in a file" {
        It "All errors are found in a file" {
            # this is a script with 12 parse errors
            1..12 | ForEach-Object { ');' } | Out-File -Encoding ASCII "${TestDrive}\badfile.ps1"
            $moreThanTenErrors = Invoke-ScriptAnalyzer -Path "${TestDrive}\badfile.ps1"
            @($moreThanTenErrors).Count | Should -Be 12
        }
    }

    Context "DiagnosticRecord  " {
        It "has valid ScriptPath and ScriptName properties when an input file is given" {
            $scriptName = "TestScript.ps1"
            $scriptPath = Join-Path $PSScriptRoot $scriptName
            $expectedScriptPath = Resolve-Path $PSScriptRoot\TestScript.ps1
            $diagnosticRecords = Invoke-ScriptAnalyzer $scriptPath -IncludeRule "PSAvoidUsingEmptyCatchBlock"
            $diagnosticRecords[0].ScriptPath | Should -Be $expectedScriptPath.Path
            $diagnosticRecords[0].ScriptName | Should -Be $scriptName
        }

        It "has empty ScriptPath and ScriptName properties when a script definition is given" {
            $diagnosticRecords = Invoke-ScriptAnalyzer -ScriptDefinition gci -IncludeRule "PSAvoidUsingCmdletAliases"
            $diagnosticRecords[0].ScriptPath | Should -Be ([System.String]::Empty)
            $diagnosticRecords[0].ScriptName | Should -Be ([System.String]::Empty)
        }
    }

    Context "When given a glob" -Skip:$testingLibraryUsage {
        It "Invokes on all the matching files" {
            $numFilesResult = (Invoke-ScriptAnalyzer -Path $PSScriptRoot\TestTestPath*.ps1 | Select-Object -Property ScriptName -Unique).Count
            $numFilesExpected = (Get-ChildItem -Path $PSScriptRoot\TestTestPath*.ps1).Count
            $numFilesResult | Should -Be $numFilesExpected
        }
    }

    Context "When given a FileSystem PSDrive" -Skip:$testingLibraryUsage {
        BeforeAll {
            #There is probably a more concise way to do this but for now we will settle for this!
            Function GetFreeDrive ($freeDriveLen) {
                $ordA = 65
                $ordZ = 90
                $freeDrive = ""
                $freeDriveName = ""
                do{
                    $freeDriveName = (1..$freeDriveLen | %{[char](Get-Random -Maximum $ordZ -Minimum $ordA)}) -join ''
                    $freeDrive = $freeDriveName + ":"
                }while (Test-Path $freeDrive)
                $freeDrive, $freeDriveName
            }
        }

        It "Recognizes the path" {
            $freeDriveNameLen = 2
            $freeDrive, $freeDriveName = GetFreeDrive $freeDriveNameLen
            New-PSDrive -Name $freeDriveName -PSProvider FileSystem -Root $PSScriptRoot
            $numFilesExpected = (Get-ChildItem -Path $freeDrive\TestTestPath*.ps1).Count
            $numFilesResult = (Invoke-ScriptAnalyzer -Path $freeDrive\TestTestPath*.ps1 | Select-Object -Property ScriptName -Unique).Count
            Remove-PSDrive $freeDriveName
            $numFilesResult | Should -Be $numFilesExpected
        }
    }

    Context "When piping in files" -Skip:$testingLibraryUsage {
        It "Can be piped in from a string" {
            $piped = ("$PSScriptRoot\TestScript.ps1" | Invoke-ScriptAnalyzer)
            $explicit = Invoke-ScriptAnalyzer -Path $PSScriptRoot\TestScript.ps1

            $piped.Count | Should -Be $explicit.Count
        }

        It "Can be piped from Get-ChildItem" {
            $piped = ( Get-ChildItem -Path $PSScriptRoot -Filter TestTestPath*.ps1 | Invoke-ScriptAnalyzer)
            $explicit = Invoke-ScriptAnalyzer -Path $PSScriptRoot\TestTestPath*.ps1
            $piped.Count | Should -Be $explicit.Count
        }
    }

    Context "When given a directory" {
        BeforeAll {
            $withoutPathWithDirectory = Invoke-ScriptAnalyzer -Recurse $PSScriptRoot\RecursionDirectoryTest
            $withPathWithDirectory = Invoke-ScriptAnalyzer -Recurse -Path $PSScriptRoot\RecursionDirectoryTest
        }

        It "Has the same count as without Path parameter" {
            $withoutPathWithDirectory.Count | Should -Be $withPathWithDirectory.Count
        }

        It "Analyzes all the files" {
            $globalVarsViolation = $withPathWithDirectory | Where-Object { $_.RuleName -eq "PSAvoidGlobalVars" }
            $clearHostViolation = $withPathWithDirectory | Where-Object { $_.RuleName -eq "PSAvoidUsingClearHost" }
            $writeHostViolation = $withPathWithDirectory | Where-Object { $_.RuleName -eq "PSAvoidUsingWriteHost" }
            Write-Output $globalVarsViolation.Count
            Write-Output $clearHostViolation.Count
            Write-Output $writeHostViolation.Count
            $globalVarsViolation.Count -eq 1 -and $writeHostViolation.Count -eq 1 | Should -BeTrue
        }
    }
}

Describe "Test ExcludeRule" {
    Context "When used correctly" {
        It "excludes 1 rule" {
            $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -ExcludeRule $singularNouns | Where-Object { $_.RuleName -eq $singularNouns }
            $noViolations.Count | Should -Be 0
        }

        It "excludes 3 rules" {
            $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -ExcludeRule $rules | Where-Object { $rules -contains $_.RuleName }
            $noViolations.Count | Should -Be 0
        }
    }

    Context "When used incorrectly" {
        It "does not exclude any rules" {
            $noExclude = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1
            $withExclude = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -ExcludeRule "This is a wrong rule"
            $withExclude.Count -eq $noExclude.Count | Should -BeTrue
        }
    }

    Context "Support wild card" {
        It "supports wild card exclusions of input rules" {
            $excludeWildCard = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -ExcludeRule $avoidRules | Where-Object { $_.RuleName -match $avoidRules }
        }
    }
}

Describe "Test IncludeRule" {
    Context "When used correctly" {
        It "includes 1 rule" {
            $violations = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -IncludeRule $approvedVerb | Where-Object { $_.RuleName -eq $approvedVerb }
            $violations.Count | Should -Be 1
        }

        It "includes the given rules" {
            # CoreCLR version of PSScriptAnalyzer does not contain PSUseSingularNouns rule
            $expectedNumViolations = 2
            if ($IsCoreCLR)
            {
                $expectedNumViolations = 1
            }
            $violations = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -IncludeRule $rules
            $violations.Count | Should -Be $expectedNumViolations
        }
    }

    Context "When used incorrectly" {
        It "does not include any rules" {
            $wrongInclude = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -IncludeRule "This is a wrong rule"
            $wrongInclude.Count | Should -Be 0
        }
    }

    Context "IncludeRule supports wild card" {
        It "includes 1 wildcard rule" {
            $includeWildcard = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -IncludeRule $avoidRules
            $includeWildcard.Count | Should -Be 0
        }

        It "includes 2 wildcardrules" {
            # CoreCLR version of PSScriptAnalyzer does not contain PSUseSingularNouns rule
            $expectedNumViolations = 4
            if ($IsCoreCLR)
            {
                $expectedNumViolations = 3
            }
            $includeWildcard = Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -IncludeRule $avoidRules
            $includeWildcard += Invoke-ScriptAnalyzer $PSScriptRoot\..\Rules\BadCmdlet.ps1 -IncludeRule $useRules
            $includeWildcard.Count | Should -Be $expectedNumViolations
        }
    }
}

Describe "Test Exclude And Include" {
    It "Exclude and Include different rules" {
        $violations = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -IncludeRule "PSAvoidUsingEmptyCatchBlock" -ExcludeRule "PSAvoidUsingPositionalParameters"
        $violations.Count | Should -Be 1
    }

    It "Exclude and Include the same rule" {
        $violations = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -IncludeRule "PSAvoidUsingEmptyCatchBlock" -ExcludeRule "PSAvoidUsingEmptyCatchBlock"
        $violations.Count | Should -Be 0
    }
}

Describe "Test Severity" {
    Context "Each severity can be chosen in any combination" {
        BeforeAll {
            $Severities = "ParseError", "Error", "Warning", "Information"
            # end space is important
            $script = '$a=;ConvertTo-SecureString -Force -AsPlainText "bad practice" '
        }

        It "Can retrieve specific severity <Severity>" {
            param ( $Severity )
            $result = Invoke-ScriptAnalyzer -ScriptDefinition $script -Severity $Severity
            if ( $Severity -is [array] ) {
                @($result).Count | Should -Be @($Severity).Count
                foreach ( $sev in $Severity ) {
                    $result.Severity | Should -Contain $sev
                }
            }
            else {
                $result.Severity | Should -Be $severity
            }
        } -TestCases @(
            @{ Severity = 'ParseError' }
            @{ Severity = 'Error' }
            @{ Severity = 'Warning' }
            @{ Severity = 'Information' }
            @{ Severity = 'ParseError', 'Error' }
            @{ Severity = 'ParseError', 'Information' }
            @{ Severity = 'Information', 'Warning', 'Error' }
        )
    }

    Context "When used correctly" {
        It "works with one argument" {
            $errors = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -Severity Information
            $errors.Count | Should -Be 0
        }

        It "works with 2 arguments" {
            $errors = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -Severity Information, Warning
            $errors.Count | Should -Be 1
        }

        It "works with lowercase argument" {
            $errors = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -Severity information, warning
            $errors.Count | Should -Be 1
        }

        It "works for dsc rules" {
            $testDataPath = [System.IO.Path]::Combine($(Split-Path $PSScriptRoot -Parent), `
                    'Rules', `
                    'DSCResourceModule', `
                    'DSCResources', `
                    'MSFT_WaitForAll', `
                    'MSFT_WaitForAll.psm1')

            Function Get-Count { begin { $count = 0 } process { $count++ } end { $count } }

            Invoke-ScriptAnalyzer -Path $testDataPath -Severity Error | `
                    Where-Object { $_.RuleName -eq "PSDSCUseVerboseMessageInDSCResource" } | `
                    Get-Count | `
                    Should -Be 0

            Invoke-ScriptAnalyzer -Path $testDataPath -Severity Information | `
                    Where-Object { $_.RuleName -eq "PSDSCUseVerboseMessageInDSCResource" } | `
                    Get-Count | `
                    Should -Be 2
        }
    }

    Context "When used incorrectly" {
        It "throws error" {
            { Invoke-ScriptAnalyzer -Severity "Wrong" $PSScriptRoot\TestScript.ps1 } | Should -Throw
        }
    }
}

Describe "Test CustomizedRulePath" {
    BeforeAll {
        $measureRequired = "CommunityAnalyzerRules\Measure-RequiresModules"
    }
    Context "When used correctly" {
        It "with the module folder path" {
            $customizedRulePath = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomizedRulePath $PSScriptRoot\CommunityAnalyzerRules | Where-Object { $_.RuleName -eq $measureRequired }
            $customizedRulePath.Count | Should -Be 1
        }

        It "with the psd1 path" {
            $customizedRulePath = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomizedRulePath $PSScriptRoot\CommunityAnalyzerRules\CommunityAnalyzerRules.psd1 | Where-Object { $_.RuleName -eq $measureRequired }
            $customizedRulePath.Count | Should -Be 1

        }

        It "with the psm1 path" {
            $customizedRulePath = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomizedRulePath $PSScriptRoot\CommunityAnalyzerRules\CommunityAnalyzerRules.psm1 | Where-Object { $_.RuleName -eq $measureRequired }
            $customizedRulePath.Count | Should -Be 1
        }

        It "with IncludeRule" {
            $customizedRulePathInclude = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomizedRulePath $PSScriptRoot\CommunityAnalyzerRules\CommunityAnalyzerRules.psm1 -IncludeRule "Measure-RequiresModules"
            $customizedRulePathInclude.Count | Should -Be 1
        }

        It "with ExcludeRule" {
            $customizedRulePathExclude = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomizedRulePath $PSScriptRoot\CommunityAnalyzerRules\CommunityAnalyzerRules.psm1 -ExcludeRule "Measure-RequiresModules" | Where-Object { $_.RuleName -eq $measureRequired }
            $customizedRulePathExclude.Count | Should -Be 0
        }

        It "When supplied with a collection of paths" {
            $customizedRulePath = Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomRulePath ("$PSScriptRoot\CommunityAnalyzerRules", "$PSScriptRoot\samplerule", "$PSScriptRoot\samplerule\samplerule2")
            $customizedRulePath.Count | Should -Be 3
        }
    }

    Context "When used from settings file" -Skip:$testingLibraryUsage {
        It "Should process relative settings path" {
            try {
                Push-Location $PSScriptRoot
                $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'gci' -Settings .\SettingsTest\..\SettingsTest\Project1\PSScriptAnalyzerSettings.psd1
                $warnings.Count | Should -Be 1
            }
            finally {
                Pop-Location
            }
        }

        It "Should process relative settings path even when settings path object is not resolved to a string yet" {
            try {
                Push-Location $PSScriptRoot
                $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'gci' -Settings (Join-Path (Get-Location).Path '.\SettingsTest\..\SettingsTest\Project1\PSScriptAnalyzerSettings.psd1')
                $warnings.Count | Should -Be 1
            }
            finally {
                Pop-Location
            }
        }

        It "resolves rule preset when passed in via pipeline" {
            $warnings = 'CodeFormattingStroustrup' | ForEach-Object {
                Invoke-ScriptAnalyzer -ScriptDefinition 'if ($true){ }' -Settings $_ }
            $warnings.Count | Should -Be 1
            $warnings.RuleName | Should -Be 'PSUseConsistentWhitespace'
        }

        It "Should use the CustomRulePath parameter" {
            $settings = @{
                CustomRulePath        = "$PSScriptRoot\CommunityAnalyzerRules"
                IncludeDefaultRules   = $false
                RecurseCustomRulePath = $false
            }

            $v = Invoke-ScriptAnalyzer -Path $PSScriptRoot\TestScript.ps1 -Settings $settings
            $v.Count | Should -Be 1
        }

        It "Should use the IncludeDefaultRulePath parameter" {
            $settings = @{
                CustomRulePath        = "$PSScriptRoot\CommunityAnalyzerRules"
                IncludeDefaultRules   = $true
                RecurseCustomRulePath = $false
            }

            $v = Invoke-ScriptAnalyzer -Path $PSScriptRoot\TestScript.ps1 -Settings $settings
            $v.Count | Should -Be 2
        }

        It "Should use the RecurseCustomRulePath parameter" {
            $settings = @{
                CustomRulePath        = "$PSScriptRoot\samplerule"
                IncludeDefaultRules   = $false
                RecurseCustomRulePath = $true
            }

            $v = Invoke-ScriptAnalyzer -Path $PSScriptRoot\TestScript.ps1 -Settings $settings
            $v.Count | Should -Be 3
        }
    }

    Context "When used from settings file and command line simulataneusly" -Skip:$testingLibraryUsage {
        BeforeAll {
            $settings = @{
                CustomRulePath        = "$PSScriptRoot\samplerule"
                IncludeDefaultRules   = $false
                RecurseCustomRulePath = $false
            }
            $isaParams = @{
                Path     = "$PSScriptRoot\TestScript.ps1"
                Settings = $settings
            }
        }

        It "Should combine CustomRulePaths" {
            $v = Invoke-ScriptAnalyzer @isaParams -CustomRulePath "$PSScriptRoot\CommunityAnalyzerRules"
            $v.Count | Should -Be 2
        }

        It "Should override the settings IncludeDefaultRules parameter" {
            $v = Invoke-ScriptAnalyzer @isaParams -IncludeDefaultRules
            $v.Count | Should -Be 2
        }

        It "Should override the settings RecurseCustomRulePath parameter" {
            $v = Invoke-ScriptAnalyzer @isaParams -RecurseCustomRulePath
            $v.Count | Should -Be 3
        }
    }

    Context "When used incorrectly" {
        It "file cannot be found" {
            try
            {
                Invoke-ScriptAnalyzer $PSScriptRoot\TestScript.ps1 -CustomRulePath "Invalid CustomRulePath"
            }
            catch
            {
                if (-not $testingLibraryUsage)
                {
                    $_.FullyQualifiedErrorId | Should -Match "PathNotFound,Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands.InvokeScriptAnalyzerCommand"
                }
            }
        }
    }
}

Describe "Test -Fix Switch" {

    BeforeAll {
        $scriptName = "TestScriptWithFixableWarnings.ps1"
        $testSource = Join-Path $PSScriptRoot $scriptName
        $fixedScript = Join-Path $PSScriptRoot TestScriptWithFixableWarnings_AfterFix.ps1
        $expectedScriptContent = Get-Content $fixedScript -Raw
        $testScript = Join-Path $TESTDRIVE $scriptName
    }

    BeforeEach {
        Copy-Item $testSource $TESTDRIVE
    }

    It "Fixes warnings" {
        # we expect the script to contain warnings
        $warningsBeforeFix = Invoke-ScriptAnalyzer $testScript
        $warningsBeforeFix.Count | Should -Be 5

        # fix the warnings and expect that it should not return the fixed warnings
        $warningsWithFixSwitch = Invoke-ScriptAnalyzer $testScript -Fix
        $warningsWithFixSwitch.Count | Should -Be 0

        # double check that the warnings are really fixed
        $warningsAfterFix = Invoke-ScriptAnalyzer $testScript
        $warningsAfterFix.Count | Should -Be 0

        # check content to ensure we have what we expect
        $actualScriptContentAfterFix = Get-Content $testScript -Raw
        $actualScriptContentAfterFix | Should -Be $expectedScriptContent
    }
}

Describe "Test -EnableExit Switch" {
    It "Returns exit code equivalent to number of warnings" {
        if ($IsCoreCLR)
        {
            $pwshExe = (Get-Process -Id $PID).Path
        }
        else
        {
            $pwshExe = 'powershell'
        }

        & $pwshExe -Command 'Import-Module PSScriptAnalyzer; Invoke-ScriptAnalyzer -ScriptDefinition gci -EnableExit'

        $LASTEXITCODE  | Should -Be 1
    }

    Describe "-ReportSummary switch" {
        BeforeAll {
            if ($IsCoreCLR)
            {
                $pwshExe = (Get-Process -Id $PID).Path
            }
            else
            {
                $pwshExe = 'powershell'
            }

            $reportSummaryFor1Warning = '*1 rule violation found.    Severity distribution:  Error = 0, Warning = 1, Information = 0*'
        }

        It "prints the correct report summary using the -NoReportSummary switch" {
            $result = & $pwshExe -Command 'Import-Module PSScriptAnalyzer; Invoke-ScriptAnalyzer -ScriptDefinition gci -ReportSummary'

            "$result" | Should -BeLike $reportSummaryFor1Warning
        }
        It "does not print the report summary when not using -NoReportSummary switch" {
            $result = & $pwshExe -Command 'Import-Module PSScriptAnalyzer; Invoke-ScriptAnalyzer -ScriptDefinition gci'

            "$result" | Should -Not -BeLike $reportSummaryFor1Warning
        }
    }

    # using statements are only supported in v5+
    Describe "Handles parse errors due to unknown types" -Skip:($testingLibraryUsage -or ($PSVersionTable.PSVersion -lt '5.0')) {
        BeforeAll {
            $script = @'
                using namespace Microsoft.Azure.Commands.ResourceManager.Cmdlets.SdkModels
                using namespace Microsoft.Azure.Commands.Common.Authentication.Abstractions
                Import-Module "AzureRm"
                class MyClass { [IStorageContext]$StorageContext } # This will result in a parser error due to [IStorageContext] type that comes from the using statement but is not known at parse time
'@
        }
        It "does not throw and detect one expected warning after the parse error has occured when using -ScriptDefintion parameter set" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition $script
            $warnings.Count | Should -Be 1
            $warnings.RuleName | Should -Be 'TypeNotFound'
        }

        It "does not throw and detect one expected warning after the parse error has occured when using -Path parameter set" {
            $testFilePath = "TestDrive:\testfile.ps1"
            Set-Content $testFilePath -Value $script
            $warnings = Invoke-ScriptAnalyzer -Path $testFilePath
            $warnings.Count | Should -Be 1
            $warnings.RuleName | Should -Be 'TypeNotFound'
        }
    }

    Describe 'Handles static Singleton (issue 1182)' -Skip:($testingLibraryUsage -or ($PSVersionTable.PSVersion -lt '5.0')) {
        It 'Does not throw or return diagnostic record' {
            $scriptDefinition = 'class T { static [T]$i }; function foo { [CmdletBinding()] param () $script:T.WriteLog() }'
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -ErrorAction Stop | Should -BeNullOrEmpty
        }
    }
}
