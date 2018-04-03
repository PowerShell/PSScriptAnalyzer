$ruleName = "PSUseCompatibleTypes"
$directory = Split-Path $MyInvocation.MyCommand.Path -Parent
$testRootDirectory = Split-Path -Parent $directory
$ruleTestDirectory = Join-Path $directory 'UseCompatible/Types'
$violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
$settingsFilePath =  [System.IO.Path]::Combine($ruleTestDirectory, 'PSScriptAnalyzerSettings.psd1');

#Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

Describe "UseCompatibleTypes" {
    Context "script has violation" {
        It "detects violation" {
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -IncludeRule $ruleName -Settings $settingsFilePath
            $diagnosticRecords.Count | Should Be 2
        }
    }

    Function Test-Command
    {
        param (
            [Parameter(ValueFromPipeline)]
            $command,
            $expectedViolations
        )
        process
        {
            It ("found {0} violations for '{1}'" -f $expectedViolations, $command) {
                Invoke-ScriptAnalyzer -ScriptDefinition $command -IncludeRule $ruleName -Settings $settingsFilePath | `
                    Get-Count | `
                    Should Be $expectedViolations
            }
        }
    }

    Context "Microsoft.PowerShell.Core" {
         @('string', 'BindingContext', 'System.Security.Policy.Url') | `
            Test-Command -ExpectedViolations 0
    }

    Context "Non-builtin types" {
        @('typeFoo', 'madeuptype', 'Xbarx') | `
            Test-Command -ExpectedViolations 0
    }

    Context "Type Accelerators" {
        @('ref', 'float', 'xml') | `
            Test-Command -ExpectedViolations 0
    }

    Context "Types present in reference platform but not in target platform" {
        @("[System.Windows.ResourceKey]::new()",
          "[Microsoft.SqlServer.TransactSql.ScriptDom.TableReference]::new()",
          "[Microsoft.Build.Framework.MessageImportance]::new()") | `
            Test-Command -ExpectedViolations 1
    }

    Context "Non-valid types" {
        @("")

    }

    Context "New-Object valid types" {
        @("`$param = new-object System.Management.Automation.RuntimeDefinedParameter('ConfigFile', [String], `$attributes)",
          "New-Object -TypeName System.Management.Automation.ParameterAttribute") | `
          Test-Command -ExpectedViolations 0
    }

    Context "New-Object non-valid types" {
        @("`$param = new-object System.Management.Automation.RuntimeDefinedParameter('ConfigFile', [myType], `$attributes)",
          "New-Object -TypeName 't t t'",
          "New-Object -TypeName System.Management.Automation.ParameterAttributeXX") | `
          Test-Command -ExpectedViolations 1
    }

    Context "Nested types (valid types)" {
        @("`$test = [System.Collections.Generic.Dictionary[Int32, System.Collections.Generic.List[String]]]::new()",
          "[System.Collections.ObjectModel.Collection[System.Attribute]]::new()",
          "`$output = [System.Management.Automation.PSDataCollection[PSObject]]::new()") | `
          Test-Command -ExpectedViolations 0
    }

    Context "Nested types (non-valid types)" {
        @("`$test = [System.Collections.Generic.Dictionary[Int32, System.Collections.Generic.List[TestType]]]::new()") | `
        Test-Command -ExpectedViolations 3
    }

    Context "Nested types (non-valid types)" {
        @("[System.Collections.ObjectModelTEST.Collection[System.Attribute]]::new()") | `
        Test-Command -ExpectedViolations 1
    }

    Context "Nested types (non-valid types)" {
        @("`$output = [System.Management.Automation.PSDataCollection[PSObjectSSSS]]::new()") | `
        Test-Command -ExpectedViolations 2
    }

    Context "Nested types (non-valid types)" {
        @("`$output = [System.Management.Automation.PSDataCollection[Int32, System.Collections.Generic.List[[System.Collections.Generic.Dictionary[Int32, TestType, [System.Collections.Generic.Dictionary[string, Int32]]]]]]]::new()") | `
        Test-Command -ExpectedViolations 4
    }

    Context "User created types" {
        @("new-object Bug378368.Test",
          "`$result = [ComputerManagement.DeleteRestorePoint]::SRRemoveRestorePoint(`$null)",
          "[WinBlue445735.WinBlue445735Type] `$v1 = 1") | `
          Test-Command -ExpectedViolations 1
    }

    Context "User defined types" {
        @("Class Car { [string]`$vin; }; `$car = New-Object Car -Property @{vin=1234};"
          "Class Car { [string]`$vin; }; `$custom = [Car]::new()") | `
          Test-Command -ExpectedViolations 0
    }
}
