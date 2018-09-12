$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$ruleName = 'PSDSCUseIdenticalMandatoryParametersForDSC'
$resourceBasepath = "$directory\DSCResourceModule\DSCResources"
$badResourceFilepath = [System.IO.Path]::Combine(
    $resourceBasepath,
    'MSFT_WaitForAnyNoIdenticalMandatoryParameter',
    'MSFT_WaitForAnyNoIdenticalMandatoryParameter.psm1');
$goodResourceFilepath = [System.IO.Path]::Combine($resourceBasepath,'MSFT_WaitForAny','MSFT_WaitForAny.psm1');


Describe "UseIdenticalMandatoryParametersForDSC" {
    Context "When a mandatory parameters are not present" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -Path $badResourceFilepath -IncludeRule $ruleName
        }

        It "Should find a violations" -skip:($IsLinux -or $IsMacOS) {
            $violations.Count | Should -Be 5
        }

        It "Should mark only the function name" -skip:($IsLinux -or $IsMacOS) {
            $violations[0].Extent.Text | Should -Be 'Get-TargetResource'
        }
    }

    Context "When all mandatory parameters are present" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -Path $goodResourceFilepath -IncludeRule $ruleName
        }

        # todo add a test to check one violation per function
        It "Should find a violations" -pending {
            $violations.Count | Should -Be 0
        }
    }

    Context "When a CIM class has no parent" {
        # regression test for #982 - just check no uncaught exception
        It "Should find no violations, and throw no exceptions" -skip:($IsLinux -or $IsMacOS) {

            # Arrange test content in testdrive
            $dscResources = Join-Path -Path "TestDrive:" -ChildPath "DSCResources"
            $noparentClassDir = Join-Path -Path $dscResources "ClassWithNoParent"

            # need a fake module
            $fakeModulePath = Join-Path -Path "TestDrive:" -ChildPath "test.psd1"
            Set-Content -Path $fakeModulePath -Value @"
@{
    ModuleVersion = '1.0'
    GUID = 'f5e6cc2a-5500-4592-bbe2-ef033754b56f'
    Author = 'test'

    FunctionsToExport = @()
    CmdletsToExport = @()
    VariablesToExport = '*'
    AliasesToExport = @()

    # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData = @{
        PSData = @{
        } # End of PSData hashtable
    } # End of PrivateData hashtable
}
"@
            # and under it a directory called dscresources\something
            New-Item -ItemType Directory -Path $noParentClassDir
            $noparentClassFilepath = Join-Path -Path $noParentClassDir -ChildPath 'ClassWithNoParent.psm1'
            $noparentClassMofFilepath = Join-Path -Path $noParentClassDir -ChildPath 'ClassWithNoParent.schema.mof'

            # containing a .psm1 file and a .schema.mof file with same base name
            Set-Content -Path $noParentClassFilepath -Value "#requires -Version 4.0 -Modules CimCmdlets" # the file content doesn't much matter

            Set-Content -Path $noParentClassMofFilePath -Value @"
[ClassVersion("1.0.0")]
class ClassWithNoParent
{
    [Write] Boolean Anonymous;
};
"@

            # Act - run scriptanalyzer
            $violations = Invoke-ScriptAnalyzer -Path $noParentClassFilepath -IncludeRule $ruleName -ErrorAction Stop
            $violations.Count | Should -Be 0
        }
    }
}
