# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    function Get-TypeNameAstFromScript
    {
        param([string]$Script)

        $ast = [System.Management.Automation.Language.Parser]::ParseInput($Script, [ref]$null, [ref]$null)
        $typeExpAst = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.TypeExpressionAst]
        }, $true)

        return $typeExpAst.TypeName
    }

    function Get-TypeAccelerators
    {
        [psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators', 'nonpublic')::Get.GetEnumerator()
    }
}


Describe "Type name serialization" {

    It "Serializes the name of type <InputType> to <ExpectedName>" -TestCases @(
        @{ InputType = [System.Reflection.Assembly]; ExpectedName = "System.Reflection.Assembly" }
        @{ InputType = [string]; ExpectedName = "System.String" }
        @{ InputType = [datetime]; ExpectedName = "System.DateTime" }
        @{ InputType = [string[]]; ExpectedName = "System.String[]" }
        @{ InputType = [System.TimeZoneInfo+AdjustmentRule]; ExpectedName = "System.TimeZoneInfo+AdjustmentRule" }
        @{ InputType = [System.Func`1]; ExpectedName = "System.Func``1" }
        @{ InputType = [System.Collections.Generic.Dictionary`2]; ExpectedName = "System.Collections.Generic.Dictionary``2" }
        @{ InputType = [System.Collections.Generic.Dictionary`2+Enumerator]; ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator" }
        @{ InputType = [System.Collections.Generic.Dictionary[string, object]].GetNestedType('Enumerator'); ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator" }
        @{ InputType = [System.Collections.Generic.List[object]]; ExpectedName = "System.Collections.Generic.List``1[System.Object]" }
        @{ InputType = [System.Collections.Generic.Dictionary[string, object]]; ExpectedName = "System.Collections.Generic.Dictionary``2[System.String,System.Object]" }
        @{ InputType = [System.Collections.Generic.Dictionary`2+Enumerator[string, object]]; ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator[System.String,System.Object]" }
        @{ InputType = [System.Collections.Concurrent.ConcurrentDictionary`2].GetMethod('ToArray').ReturnType; ExpectedName = "System.Collections.Generic.KeyValuePair``2[]" }
    ) {
        $name = [Microsoft.PowerShell.CrossCompatibility.TypeNaming]::GetFullTypeName($InputType)
        $name | Should -BeExactly $ExpectedName
    }

    It "Null type throws exception" {
        {
            [Microsoft.PowerShell.CrossCompatibility.TypeNaming]::GetFullTypeName($null)
        } | Should -Throw -ErrorId "ArgumentNullException"
    }

    It "Strips generic quantifiers from '<RawTypeName>' to return '<StrippedTypeName>'" {
        param([string]$RawTypeName, [string]$StrippedTypeName)

        $stripped = [Microsoft.PowerShell.CrossCompatibility.TypeNaming]::StripGenericQuantifiers($RawTypeName)
        $stripped | Should -BeExactly $StrippedTypeName
    } -TestCases @(
        @{ RawTypeName = "String"; StrippedTypeName = "String" }
        @{ RawTypeName = "Dictionary``2"; StrippedTypeName = "Dictionary" }
        @{ RawTypeName = "Dictionary``2"; StrippedTypeName = "Dictionary" }
        @{ RawTypeName = "Dictionary``2+Enumerator"; StrippedTypeName = "Dictionary+Enumerator" }
    )
}

Describe "Type accelerator expansion" {
    BeforeAll {
        $typeAccelerators = Get-TypeAccelerators |
            ForEach-Object { $d = New-Object 'System.Collections.Generic.Dictionary[string,string]' } { $d.Add($_.Key, $_.Value.FullName) } { $d }
    }

    It "Expands the typename in <Raw> to <Expanded>" -TestCases @(
        @{ Raw = "[System.Exception]"; Expanded = "System.Exception" }
        @{ Raw = "[string]"; Expanded = "System.String" }
        @{ Raw = "[psmoduleinfo]"; Expanded = "System.Management.Automation.PSModuleInfo" }
        @{ Raw = "[System.Collections.Generic.List[int]]"; Expanded = "System.Collections.Generic.List``1[System.Int32]" }
        @{ Raw = "[System.Collections.Generic.Dictionary[string,psmoduleinfo]]"; Expanded = "System.Collections.Generic.Dictionary``2[System.String,System.Management.Automation.PSModuleInfo]" }
        @{ Raw = "[System.Collections.Generic.Dictionary[string, psmoduleinfo]]"; Expanded = "System.Collections.Generic.Dictionary``2[System.String,System.Management.Automation.PSModuleInfo]" }
        @{ Raw = "[System.Collections.Generic.Dictionary  [string,  psmoduleinfo]]"; Expanded = "System.Collections.Generic.Dictionary``2[System.String,System.Management.Automation.PSModuleInfo]" }
        @{ Raw = "[System.Collections.Generic.List``1[uri]]"; Expanded = "System.Collections.Generic.List``1[System.Uri]" }
        @{ Raw = "[System.Collections.Generic.Dictionary``2[string,psmoduleinfo]]"; Expanded = "System.Collections.Generic.Dictionary``2[System.String,System.Management.Automation.PSModuleInfo]" }
        @{ Raw = "[System.Collections.Generic.Dictionary``2  [string, psmoduleinfo]]"; Expanded = "System.Collections.Generic.Dictionary``2[System.String,System.Management.Automation.PSModuleInfo]" }
        @{ Raw = "[object]"; Expanded = "System.Object" }
    ) -Test {

        $typeName = Get-TypeNameAstFromScript -Script $Raw

        $canonicalName = [Microsoft.PowerShell.CrossCompatibility.TypeNaming]::GetCanonicalTypeName($typeAccelerators, $typeName)

        $canonicalName | Should -BeExactly $Expanded
    }
}

$versionCreationTests = @(
    @{ Version = '6.1'; Major = 6; Minor = 1; Patch = -1 }
    @{ Version = '6.1.4'; Major = 6; Minor = 1; Patch = 4; }
    @{ Version = '5.1.8-preview.2'; Major = 5; Minor = 1; Patch = 8; Label = 'preview.2' }
    @{ Version = [version]'4.2'; Major = 4; Minor = 2; Patch = -1; Revision = -1 }
    @{ Version = [version]'4.2.1'; Major = 4; Minor = 2; Patch = 1; Revision = -1 }
    @{ Version = [version]'4.2.1.7'; Major = 4; Minor = 2; Patch = 1; Revision = 7 }
)

if ($PSVersionTable.PSVersion.Major -ge 6)
{
    $versionCreationTests += @(
        @{ Version = [semver]'6.1.2'; Major = 6; Minor = 1; Patch = 2; Label = $null }
        @{ Version = [semver]'6.1.2-rc.1'; Major = 6; Minor = 1; Patch = 2; Label = 'rc.1' }
        @{ Version = [semver]'6.1-rc.1'; Major = 6; Minor = 1; Patch = 0; Label = 'rc.1' }
        @{ Version = [semver]'6-rc.1'; Major = 6; Minor = 0; Patch = 0; Label = 'rc.1' }
        @{ Version = [semver]'6.1.2-rc.1+duck'; Major = 6; Minor = 1; Patch = 2; Label = 'rc.1'; BuildLabel = 'duck' }
        @{ Version = [semver]'6.1-rc.1+duck'; Major = 6; Minor = 1; Patch = 0; Label = 'rc.1'; BuildLabel = 'duck' }
        @{ Version = [semver]'6-rc.1+duck'; Major = 6; Minor = 0; Patch = 0; Label = 'rc.1'; BuildLabel = 'duck' }
    )
}

Describe "PowerShell version object" {
    Context "Version parsing" {

        It "Parses version string '<VerStr>' as <Major>.<Minor>.<Patch>" -TestCases @(
            @{ VerStr = '42'; Major = 42; Minor = -1; Patch = -1 }
            @{ VerStr = '7'; Major = 7; Minor = -1; Patch = -1 }
            @{ VerStr = '6.1'; Major = 6; Minor = 1; Patch = -1 }
            @{ VerStr = '5.2.7'; Major = 5; Minor = 2; Patch = 7 }
            @{ VerStr = '512.2124.71'; Major = 512; Minor = 2124; Patch = 71 }
        ) -Test {

            $v = [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]$VerStr

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
        }

        It "Parses version string '<VerStr>' as <Major>.<Minor>.<Patch>-<Label>+<BuildLabel>" -TestCases @(
            @{ VerStr = '6.1.0-rc.1'; Major = 6; Minor = 1; Patch = 0; Label = 'rc.1' }
            @{ VerStr = '6.2-preview.2'; Major = 6; Minor = 2; Patch = -1; Label = 'preview.2' }
            @{ VerStr = '6-preview.2'; Major = 6; Minor = -1; Patch = -1; Label = 'preview.2' }
            @{ VerStr = '6.1.0-rc.1+moo'; Major = 6; Minor = 1; Patch = 0; Label = 'rc.1'; BuildLabel = 'moo' }
            @{ VerStr = '6.2-preview.2+horse'; Major = 6; Minor = 2; Patch = -1; Label = 'preview.2'; BuildLabel = 'horse' }
            @{ VerStr = '6-preview.2+veryimportant'; Major = 6; Minor = -1; Patch = -1; Label = 'preview.2'; BuildLabel = 'veryimportant' }
        ) -Test {
            param([string]$VerStr, [int]$Major, [int]$Minor, [int]$Patch, [string]$Label, $BuildLabel)

            $v = [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]$VerStr

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
            $v.PreReleaseLabel | Should -BeExactly $Label
            $v.BuildLabel | Should -Be $BuildLabel
        }

        It "Parses version string '<VerStr>' as <Major>.<Minor>.<Patch>.<Revision>"-TestCases @(
            @{ VerStr = '5.2.1.12312'; Major = 5; Minor = 2; Patch = 1; Revision = 12312 }
        ) -Test {
            param([string]$VerStr, [int]$Major, [int]$Minor, [int]$Patch, [int]$Revision)

            $v = [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]$VerStr

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
            $v.Revision | Should -Be $Revision
        }

        It "Does not parse '<VerStr>' as a version" -TestCases @(
            @{ VerStr = 'banana' }
            @{ VerStr = '' }
            @{ VerStr = '1.' }
            @{ VerStr = '.6' }
            @{ VerStr = '5.1.' }
            @{ VerStr = '5.1.2.' }
            @{ VerStr = '4.1.5.7.' }
            @{ VerStr = '4.1.5.7.4' }
            @{ VerStr = '4.1.5.7-rc.2' }
            @{ VerStr = '4.1.5.-rc.2' }
        ) -Test {
            param([string]$VerStr)

            { [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]$VerStr } | Should -Throw
        }
    }

    Context "Version creation from other versions" {

        It "Creates a PowerShellVersion from '<Version>'" -TestCases $versionCreationTests {
            param($Version, [int]$Major, [int]$Minor, [int]$Patch, [int]$Revision, $Label, $BuildLabel)

            $v = [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]::Create($Version)

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
            $v.PreReleaseLabel | Should -Be $Label
            $v.BuildLabel | Should -Be $BuildLabel

            if ($Revision)
            {
                $v.Revision | Should -Be $Revision
            }
        }

        It "Does not create a PowerShellVersion from <Version>" {
            param($Version)

            { [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]::Create($Version) } | Should -Throw
        } -TestCases @(
            @{ Version = $null }
            @{ Version = New-Object 'object' }
            @{ Version = 'Hello' }
        )
    }
}
