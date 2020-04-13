# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $script:fileContent = @'
    $assetDir = 'C:\ImportantFiles\'
    $archiveDir = 'C:\Archived\'

    $runningOnWindows = -not ($IsLinux -or $IsMacOS)
    $zipCompressionLevel = 'Optimal'
    $includeBaseDirInZips = $true

    if (-not (Test-Path $archiveDir))
    {
        New-Item -Type Directory $archiveDir
    }

    Import-Module -FullyQualifiedName @{ ModuleName = 'MyArchiveUtilities'; ModuleVersion = '2.0' }

    $sigs = [System.Collections.Generic.List[System.Management.Automation.Signature]]::new()
    $filePattern = [System.Management.Automation.WildcardPattern]::Get('system*')
    foreach ($file in Get-ChildItem $assetDir -Recurse -Depth 1)
    {
        if (-not $filePattern.IsMatch($file.Name))
        {
            continue
        }

        if ($file.Name -like '*.dll')
        {
            $sig = Get-AuthenticodeSignature $file
            $sigs.Add($sig)
            continue
        }

        if (Test-WithFunctionFromMyModule -File $file)
        {
            $destZip = Join-Path $archiveDir $file.BaseName
            [System.IO.Compression.ZipFile]::CreateFromDirectory($file.FullName, "$destZip.zip", $zipCompressionLevel, $includeBaseDirInZips)
        }
    }

    Write-Output $sigs
'@

    $script:settingsContent = @'
    @{
        Rules = @{
            PSUseCompatibleCommands = @{
                Enable = $true
                TargetProfiles = @(
                    'win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework' # Server 2019 - PS 5.1 (the platform it already runs on)
                    'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework' # Server 2012 - PS 3
                    'ubuntu_x64_18.04_7.0.0_x64_3.1.2_core' # Ubuntu 18.04 - PS 6.1
                )
            }
            PSUseCompatibleTypes = @{
                Enable = $true
                # Same as for command targets
                TargetProfiles = @(
                    'win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
                    'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
                    'ubuntu_x64_18.04_7.0.0_x64_3.1.2_core'
                )
            }
            PSUseCompatibleSyntax = @{
                Enable = $true
                TargetVersions = @('3.0', '5.1', '7.0')
            }
        }
    }
'@
}


Describe "Running all compatibility rules with a settings file" {
    BeforeAll {
        $testFile = New-Item -Path "$TestDrive/test.ps1" -Value $script:fileContent -ItemType File
        $settingsFile = New-Item -Path "$TestDrive/settings.psd1" -Value $script:settingsContent -ItemType File
        $diagnostics = Invoke-ScriptAnalyzer -Path $testFile.FullName -Settings $settingsFile.FullName |
            Group-Object -Property RuleName -AsHashtable
        $commandDiagnostics = $diagnostics.PSUseCompatibleCommands
        $typeDiagnostics = $diagnostics.PSUseCompatibleTypes
        $syntaxDiagnostics = $diagnostics.PSUseCompatibleSyntax
    }

    It "Finds the problem with command <Command> on line <Line> in the file" {
        param([string]$Command, [string]$Parameter, [int]$Line)

        $actualDiagnostic = $commandDiagnostics | Where-Object { $_.Command -eq $Command -and $_.Line -eq $Line }
        $actualDiagnostic.Command | Should -BeExactly $Command
        $actualDiagnostic.Line | Should -Be $Line
        if ($Parameter)
        {
            $actualDiagnostic.Parameter | Should -Be $Parameter
        }
    } -TestCases @(
        @{ Command = 'Import-Module'; Parameter = 'FullyQualifiedName'; Line = 13 }
        @{ Command = 'Get-ChildItem'; Parameter = 'Depth'; Line = 17 }
        @{ Command = 'Get-AuthenticodeSignature'; Line = 26 }
    )

    It "Finds the problem with type <Type> on line <Line> in the file" {
        param([string]$Type, [string]$Member, [int]$Line)

        $actualDiagnostic = $typeDiagnostics | Where-Object { $_.Type -eq $Type -and $_.Line -eq $Line }
        $actualDiagnostic.Type | Should -BeExactly $Type
        $actualDiagnostic.Line | Should -Be $Line
        if ($Member)
        {
            $actualDiagnostic.Member | Should -Be $Member
        }
    } -TestCases @(
        @{ Type = 'System.Management.Automation.WildcardPattern'; Member = 'Get'; Line = 16 }
        @{ Type = 'System.IO.Compression.ZipFile'; Line = 34 }
    )

    It "Finds the problem with syntax on line <Line> in the file" {
        param([string]$Suggestion, [int]$Line)

        $actualDiagnostic = $syntaxDiagnostics | Where-Object { $_.Line -eq $Line }
        $actualDiagnostic.Line | Should -Be $Line
        if ($Suggestion)
        {
            $actualDiagnostic.Suggestion | Should -Be $Suggestion
        }
    } -TestCases @(
        @{ Line = 15; CorrectionText = "New-Object 'System.Collections.Generic.List[System.Management.Automation.Signature]'" }
    )
}