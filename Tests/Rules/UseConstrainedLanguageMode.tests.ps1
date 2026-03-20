# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
# Tests for UseConstrainedLanguageMode rule
#
# Some tests are Windows-specific (COM objects, XAML) and will be skipped on non-Windows platforms.

BeforeDiscovery {
    # Detect OS for platform-specific tests
    $script:IsWindowsOS = $true
    $script:IsLinuxOS = $false
    $script:IsMacOSOS = $false
    
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        # PowerShell Core has built-in OS detection variables
        $script:IsWindowsOS = $IsWindows
        $script:IsLinuxOS = $IsLinux
        $script:IsMacOSOS = $IsMacOS
    }
}
BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $violationName = "PSUseConstrainedLanguageMode"
    $ruleName = $violationName
    
    # The rule is disabled by default, so we need to enable it
    $settings = @{
        IncludeRules = @($ruleName)
        Rules = @{
            $ruleName = @{
                Enable = $true
            }
        }
    }
}

Describe "UseConstrainedLanguageMode" {
Context "When Add-Type is used" {
        It "Should detect Add-Type usage" {
            $def = @'
Add-Type -TypeDefinition @"
    public class TestType {
        public static string Test() { return "test"; }
    }
"@
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].RuleName | Should -Be $violationName
            $violations[0].Message | Should -BeLike "*Add-Type*"
        }

        It "Should not flag other commands" {
            $def = 'Get-Process | Where-Object { $_.Name -eq "powershell" }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }
    }

    Context "When New-Object with COM is used" {
        It "Should detect disallowed New-Object -ComObject usage" -Skip:(-not $script:IsWindowsOS) {
            $def = 'New-Object -ComObject "Excel.Application"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*COM object*"
        }

        It "Should NOT flag allowed COM objects - Scripting.Dictionary" -Skip:(-not $script:IsWindowsOS) {
            $def = 'New-Object -ComObject "Scripting.Dictionary"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should NOT flag allowed COM objects - Scripting.FileSystemObject" -Skip:(-not $script:IsWindowsOS) {
            $def = 'New-Object -ComObject "Scripting.FileSystemObject"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should NOT flag allowed COM objects - VBScript.RegExp" -Skip:(-not $script:IsWindowsOS) {
            $def = 'New-Object -ComObject "VBScript.RegExp"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should NOT flag New-Object with allowed TypeName" {
            $def = 'New-Object -TypeName System.Collections.ArrayList'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should flag New-Object with disallowed TypeName" {
            $def = 'New-Object -TypeName System.IO.File'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*System.IO.File*not permitted*"
        }
    }

    Context "When XAML is used" {
        It "Should detect XAML usage" -Skip:(-not $script:IsWindowsOS) {
            $def = @'
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Button>Click me</Button>
</Window>
"@
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*XAML*"
        }
    }

    Context "When Invoke-Expression is used" {
        It "Should detect Invoke-Expression usage" {
            $def = 'Invoke-Expression "Get-Process"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*Invoke-Expression*"
        }
    }

    Context "When dot-sourcing is used" {
        It "Should detect dot-sourcing in unsigned scripts" {
            $def = '. $PSScriptRoot\Helper.ps1'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*dot*"
        }
    }

    Context "When PowerShell classes are used" {
        It "Should detect class definition" {
            $def = @'
class MyClass {
    [string]$Name
    [int]$Value
    
    MyClass([string]$name) {
        $this.Name = $name
    }
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*class*MyClass*"
        }

        It "Should detect multiple class definitions" {
            $def = @'
class FirstClass {
    [string]$Name
}

class SecondClass {
    [int]$Value
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 2
        }

        It "Should not flag enum definitions" {
            $def = @'
enum MyEnum {
    Value1
    Value2
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            # Enums are allowed, so no class-specific violations
            # (though we may still flag other issues if present)
            $classViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*class*" 
            }
            $classViolations | Should -BeNullOrEmpty
        }
    }

    Context "When type expressions are used" {
        It "Should flag static type reference with new()" {
            $def = '$instance = [System.IO.Directory]::new()'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*System.IO.Directory*"
        }

        It "Should flag static method call on disallowed type" {
            $def = '[System.IO.File]::ReadAllText("C:\test.txt")'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*System.IO.File*"
        }

        It "Should NOT flag static reference to allowed type" {
            $def = '[string]::Empty'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $typeExprViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*type expression*string*"
            }
            $typeExprViolations | Should -BeNullOrEmpty
        }
    }

    Context "When module manifests (.psd1) are analyzed" {
        BeforeAll {
            $tempPath = Join-Path $TestDrive "TestManifests"
            New-Item -Path $tempPath -ItemType Directory -Force | Out-Null
        }

        It "Should flag wildcard in FunctionsToExport" {
            $manifestPath = Join-Path $tempPath "WildcardFunctions.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    FunctionsToExport = '*'
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*FunctionsToExport*wildcard*"
        }

                It "Should flag wildcard array in FunctionsToExport" {
            $manifestPath = Join-Path $tempPath "WildcardFunctions.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    FunctionsToExport = @('*')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*FunctionsToExport*wildcard*"
        }

        It "Should flag wildcard in CmdletsToExport" {
            $manifestPath = Join-Path $tempPath "WildcardCmdlets.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    CmdletsToExport = '*'
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*CmdletsToExport*wildcard*"
        }

        It "Should NOT flag explicit list of exports" {
            $manifestPath = Join-Path $tempPath "ExplicitExports.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    FunctionsToExport = @('Get-MyFunction', 'Set-MyFunction')
    CmdletsToExport = @('Get-MyCmdlet')
    AliasesToExport = @()
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $wildcardViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*wildcard*" 
            }
            $wildcardViolations | Should -BeNullOrEmpty
        }

        It "Should flag .ps1 file in RootModule" {
            $manifestPath = Join-Path $tempPath "ScriptRootModule.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    RootModule = 'MyModule.ps1'
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*RootModule*MyModule.ps1*"
        }

        It "Should flag .ps1 file in NestedModules" {
            $manifestPath = Join-Path $tempPath "ScriptNestedModule.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    NestedModules = @('Helper.ps1', 'Utility.psm1')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*NestedModules*Helper.ps1*"
        }

        It "Should NOT flag .psm1 or .dll modules" {
            $manifestPath = Join-Path $tempPath "BinaryModules.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    RootModule = 'MyModule.psm1'
    NestedModules = @('Helper.dll', 'Utility.psm1')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $scriptModuleViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*.ps1*" 
            }
            $scriptModuleViolations | Should -BeNullOrEmpty
        }

        It "Should flag .ps1 file in ScriptsToProcess" {
            $manifestPath = Join-Path $tempPath "ScriptsToProcess.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    ScriptsToProcess = @('Init.ps1', 'Setup.ps1')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*ScriptsToProcess*Init.ps1*"
        }

        It "Should use different error message for ScriptsToProcess" {
            $manifestPath = Join-Path $tempPath "ScriptsToProcessMessage.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    ScriptsToProcess = 'Init.ps1'
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            # ScriptsToProcess should have a specific message about caller's session state
            $matchingViolations[0].Message | Should -BeLike "*caller*session state*"
            $matchingViolations[0].Message | Should -BeLike "*Init.ps1*"
        }

        It "Should flag single-item array in ScriptsToProcess" {
            $manifestPath = Join-Path $tempPath "ScriptsToProcessArray.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    ScriptsToProcess = @('Init.ps1')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*ScriptsToProcess*Init.ps1*"
        }

        It "Should NOT flag .psm1 files in ScriptsToProcess" {
            $manifestPath = Join-Path $tempPath "ScriptsToProcessPsm1.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    ScriptsToProcess = @('Init.psm1')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $scriptViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*ScriptsToProcess*" 
            }
            $scriptViolations | Should -BeNullOrEmpty
        }

        It "Should flag both wildcard and .ps1 issues in same manifest" {
            $manifestPath = Join-Path $tempPath "MultipleIssues.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    RootModule = 'MyModule.ps1'
    FunctionsToExport = '*'
    CmdletsToExport = '*'
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should have at least 3 violations: RootModule .ps1, FunctionsToExport *, CmdletsToExport *
            $matchingViolations.Count | Should -BeGreaterOrEqual 3
        }

        It "Should flag ScriptsToProcess and RootModule with different messages" {
            $manifestPath = Join-Path $tempPath "MixedScriptFields.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    RootModule = 'MyModule.ps1'
    ScriptsToProcess = @('Init.ps1')
}
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 2

            # Check that we have both types of messages
            $scriptsToProcessMsg = $matchingViolations | Where-Object { $_.Message -like "*caller*session state*" }
            $rootModuleMsg = $matchingViolations | Where-Object { $_.Message -like "*RootModule*" -and $_.Message -notlike "*caller*session state*" }

            $scriptsToProcessMsg.Count | Should -Be 1
            $rootModuleMsg.Count | Should -Be 1

            # Verify the specific field names are mentioned
            $scriptsToProcessMsg[0].Message | Should -BeLike "*Init.ps1*"
            $rootModuleMsg[0].Message | Should -BeLike "*MyModule.ps1*"
        }
    }

    Context "Rule severity" {
        It "Should have Warning severity" {
            $def = 'Add-Type -AssemblyName System.Windows.Forms'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations[0].Severity | Should -Be 'Warning'
        }
    }

    Context "When type constraints are used" {
        It "Should flag disallowed type constraint on parameter" {
            $def = 'function Test { param([System.IO.File]$FileHelper) }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*System.IO.File*not permitted*"
        }

        It "Should flag disallowed type constraint on variable declaration" {
            $def = '[System.IO.File]$fileHelper = $null'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*System.IO.File*not permitted*"
        }

        It "Should flag disallowed type cast on variable assignment" {
            $def = '$fileHelper = [System.IO.File]$value'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*System.IO.File*not permitted*"
        }

        It "Should NOT flag allowed type constraint" {
            $def = 'function Test { param([string]$Name, [int]$Count, [hashtable]$Data) }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should NOT flag allowed type cast on variable" {
            $def = '[string]$name = $null'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should flag multiple type issues in same script" {
            $def = @'
function Test {
    param([System.IO.File]$FileHelper)
    [System.IO.Directory]$dirHelper = $null
    $pathHelper = [System.IO.Path]::new()
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should flag: 1) param type constraint, 2) variable type constraint, 3) type expression
            # Note: May also flag member access if methods/properties are called on typed variables
            $matchingViolations.Count | Should -BeGreaterOrEqual 3
        }
    }

    Context "When PSCustomObject type cast is used" {
        It "Should flag [PSCustomObject]@{} syntax" {
            $def = '$obj = [PSCustomObject]@{ Name = "Test"; Value = 42 }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -BeGreaterThan 0
            $matchingViolations[0].Message | Should -BeLike "*PSCustomObject*"
        }
        
        It "Should flag multiple [PSCustomObject]@{} instances" {
            $def = @'
$obj1 = [PSCustomObject]@{ Name = "Test1" }
$obj2 = [PSCustomObject]@{ Name = "Test2" }
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 2
        }
        
        It "Should NOT flag PSCustomObject as parameter type" {
            $def = 'function Test { param([PSCustomObject]$InputObject) }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }
        
        It "Should NOT flag New-Object PSObject" {
            $def = '$obj = New-Object PSObject -Property @{ Name = "Test" }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }
        
        It "Should NOT flag plain hashtables" {
            $def = '$obj = @{ Name = "Test"; Value = 42 }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }
        
        It "Should NOT flag [PSCustomObject] with variable (not hashtable literal)" {
            $def = '$hash = @{}; $obj = [PSCustomObject]$hash'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # This is a type cast but not the @{} literal pattern
            # Since PSCustomObject is in allowed list, this won't be flagged
            $matchingViolations | Should -BeNullOrEmpty
        }

    }

    Context "When instance methods are invoked on disallowed types" {
        It "Should flag method invocation on parameter with disallowed type constraint" {
            $def = @'
function Read-File {
    param([System.IO.File]$FileHelper, [string]$Path)
    $FileHelper.ReadAllText($Path)
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should flag both the type constraint AND the member access
            $matchingViolations.Count | Should -BeGreaterThan 1
            # At least one violation should mention ReadAllText
            ($matchingViolations.Message | Where-Object { $_ -like "*ReadAllText*" }).Count | Should -BeGreaterThan 0
        }

        It "Should flag property access on variable with disallowed type constraint" {
            $def = @'
function Test {
    param([System.IO.FileInfo]$FileHelper)
    $fullPath = $FileHelper.FullName
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should flag both the type constraint AND the member access
            $matchingViolations.Count | Should -BeGreaterThan 1
            # At least one violation should mention FullName
            ($matchingViolations.Message | Where-Object { $_ -like "*FullName*" }).Count | Should -BeGreaterThan 0
        }

        It "Should flag method invocation on typed variable assignment" {
            $def = @'
[System.IO.File]$fileHelper = $null
$result = $fileHelper.ReadAllText("C:\test.txt")
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should flag both the type constraint AND the member access
            $matchingViolations.Count | Should -BeGreaterThan 1
            # At least one violation should mention ReadAllText
            ($matchingViolations.Message | Where-Object { $_ -like "*ReadAllText*" }).Count | Should -BeGreaterThan 0
        }

        It "Should NOT flag method invocation on allowed types" {
            $def = @'
function Test {
    param([string]$Text)
    $upper = $Text.ToUpper()
    $length = $Text.Length
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }

        It "Should NOT flag static method calls on disallowed types (already caught by type expression check)" {
            $def = '[System.IO.File]::Exists("test.txt")'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should only flag once for the type expression, not for member access
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*System.IO.File*"
        }

        It "Should flag chained method calls on disallowed types" {
            $def = @'
function Test {
    param([System.IO.FileInfo]$FileHelper)
    $result = $FileHelper.OpenText().ReadToEnd()
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should flag the type constraint and at least the first member access
            $matchingViolations.Count | Should -BeGreaterThan 1
        }

        It "Should handle complex scenarios with multiple typed variables" {
            $def = @'
function Complex-Test {
    param(
        [System.IO.File]$FileHelper,
        [System.IO.Directory]$DirHelper,
        [string]$SafeString
    )
    
    $data = $FileHelper.ReadAllBytes("C:\test.bin")
    $DirHelper.GetFiles("C:\temp")
    $upper = $SafeString.ToUpper()
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            # Should flag: 2 type constraints + 2 method invocations (not SafeString)
            $matchingViolations.Count | Should -BeGreaterThan 2
        }
    }

    Context "When scripts are digitally signed" {
        BeforeAll {
            $tempPath = Join-Path $TestDrive "SignedScripts"
            New-Item -Path $tempPath -ItemType Directory -Force | Out-Null
        }

        It "Should NOT flag Add-Type in signed scripts" {
            $scriptPath = Join-Path $tempPath "SignedWithAddType.ps1"
            $scriptContent = @'
Add-Type -TypeDefinition "public class Test { }"

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $scriptPath -Value $scriptContent
            $violations = Invoke-ScriptAnalyzer -Path $scriptPath -Settings $settings
            $addTypeViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*Add-Type*" 
            }
            $addTypeViolations | Should -BeNullOrEmpty
        }

        It "Should NOT flag disallowed types in signed scripts" {
            $scriptPath = Join-Path $tempPath "SignedWithDisallowedType.ps1"
            $scriptContent = @'
$fileHelper = New-Object System.IO.FileInfo("C:\test.txt")
$data = $fileHelper.OpenText()

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $scriptPath -Value $scriptContent
            $violations = Invoke-ScriptAnalyzer -Path $scriptPath -Settings $settings
            $typeViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*FileInfo*type*" 
            }
            $typeViolations | Should -BeNullOrEmpty
        }

        It "Should NOT flag classes in signed scripts" {
            $scriptPath = Join-Path $tempPath "SignedWithClass.ps1"
            $scriptContent = @'
class MyClass {
    [string]$Name
}

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $scriptPath -Value $scriptContent
            $violations = Invoke-ScriptAnalyzer -Path $scriptPath -Settings $settings
            $classViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*class*MyClass*" 
            }
            $classViolations | Should -BeNullOrEmpty
        }

        It "Should STILL flag dot-sourcing in signed scripts" {
            $scriptPath = Join-Path $tempPath "SignedWithDotSource.ps1"
            $scriptContent = @'
. .\Helper.ps1
.      .\U  tility.ps1

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $scriptPath -Value $scriptContent
            $violations = Invoke-ScriptAnalyzer -Path $scriptPath -Settings $settings
            $dotSourceViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*dot*"
            }
            # Dot-sourcing should still be flagged even in signed scripts
            $dotSourceViolations.Count | Should -BeGreaterThan 0
        }

        It "Should STILL flag disallowed parameter types in signed scripts" {
            $scriptPath = Join-Path $tempPath "SignedWithBadParam.ps1"
            $scriptContent = @'
function Test {
    param([System.IO.File]$FileHelper)
    Write-Output "Test"
}

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $scriptPath -Value $scriptContent
            $violations = Invoke-ScriptAnalyzer -Path $scriptPath -Settings $settings
            $paramViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*File*"
            }
            # Parameter type constraints should still be flagged
            $paramViolations.Count | Should -BeGreaterThan 0
        }

        It "Should STILL flag wildcard exports in signed manifests" {
            $manifestPath = Join-Path $tempPath "SignedManifest.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    FunctionsToExport = '*'
}

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $wildcardViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*wildcard*"
            }
            # Wildcard exports should still be flagged
            $wildcardViolations.Count | Should -BeGreaterThan 0
        }

        It "Should STILL flag .ps1 modules in signed manifests" {
            $manifestPath = Join-Path $tempPath "SignedManifestWithScript.psd1"
            $manifestContent = @'
@{
    ModuleVersion = '1.0.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    RootModule = 'MyModule.ps1'
}

# SIG # Begin signature block
# MIIFFAYJKoZIhvcNAQcCoIIFBTCCBQECAQExCzAJ...
# SIG # End signature block
'@
            Set-Content -Path $manifestPath -Value $manifestContent
            $violations = Invoke-ScriptAnalyzer -Path $manifestPath -Settings $settings
            $scriptModuleViolations = $violations | Where-Object { 
                $_.RuleName -eq $violationName -and $_.Message -like "*.ps1*"
            }
            # Script modules should still be flagged
            $scriptModuleViolations.Count | Should -BeGreaterThan 0
        }
    }

    Context "Performance with large scripts" {
        It "Should handle scripts with many typed variables and member invocations efficiently" {
            # This test verifies the O(N+M) cache optimization
            # Without caching, this would be O(N*M) and very slow
            
            # Build a script with many typed variables and member invocations
            $scriptBuilder = [System.Text.StringBuilder]::new()
            [void]$scriptBuilder.AppendLine('function Test-Performance {')
            [void]$scriptBuilder.AppendLine('    param([string]$Path)')
            
            # Add 30 typed variable assignments
            for ($i = 1; $i -le 30; $i++) {
                [void]$scriptBuilder.AppendLine("    [System.IO.File]`$file$i = `$null")
            }
            
            # Add 50 member invocations (testing cache reuse)
            for ($i = 1; $i -le 50; $i++) {
                $varNum = ($i % 30) + 1
                [void]$scriptBuilder.AppendLine("    `$result$i = `$file$varNum.ReadAllText(`$Path)")
            }
            
            [void]$scriptBuilder.AppendLine('}')
            $def = $scriptBuilder.ToString()
            
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            
            # Should detect violations (30 type constraints + 50 member accesses = 80+)
            $matchingViolations.Count | Should -BeGreaterThan 50
        }

        It "Should cache results per scope correctly" {
            # Test that cache is scoped properly and doesn't leak between functions
            $def = @'
function Function1 {
    [System.IO.File]$file1 = $null
    $result1 = $file1.ReadAllText("C:\test1.txt")
}

function Function2 {
    [System.IO.Directory]$file1 = $null
    $result2 = $file1.GetFiles("C:\temp")
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            
            # Should detect violations in both functions
            # Each function has: 1 type constraint + 1 member access = 2 violations each
            $matchingViolations.Count | Should -BeGreaterOrEqual 4
            
            # Verify both File and Directory are mentioned
            $messages = $matchingViolations.Message -join ' '
            $messages | Should -BeLike "*File*"
            $messages | Should -BeLike "*Directory*"
        }
    }
}
