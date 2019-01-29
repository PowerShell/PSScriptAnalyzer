Import-Module "$PSScriptRoot/../out/CrossCompatibility" -Force -ErrorAction Stop

Describe "Type name transformation" {
    BeforeAll {
        $typeNameTestCases = @(
            @{ InputType = [System.Reflection.Assembly]; ExpectedName = "System.Reflection.Assembly" }
            @{ InputType = [string]; ExpectedName = "System.String" }
            @{ InputType = [datetime]; ExpectedName = "System.DateTime" }
            @{ InputType = [string[]]; ExpectedName = "System.String[]" }
            @{ InputType = [System.Collections.Generic.List[object]]; ExpectedName = "System.Collections.Generic.List``1[System.Object]" }
            @{ InputType = [System.Collections.Generic.Dictionary[string, object]]; ExpectedName = "System.Collections.Generic.Dictionary``2[System.String,System.Object]" }
            @{ InputType = [System.Func`1]; ExpectedName = "System.Func``1[TResult]" }
            @{ InputType = [System.Collections.Generic.Dictionary`2]; ExpectedName = "System.Collections.Generic.Dictionary``2[TKey,TValue]" }
            @{ InputType = [System.Collections.Generic.Dictionary`2+Enumerator]; ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator[TKey,TValue]" }
            @{ InputType = [System.Collections.Generic.Dictionary[string,object]].GetNestedType('Enumerator'); ExpectedName = "System.Collections.Generic.Dictionary``2+Enumerator[TKey,TValue]" }
            @{ InputType = [System.Collections.Concurrent.ConcurrentDictionary`2].GetMethod('ToArray').ReturnType; ExpectedName = "System.Collections.Generic.KeyValuePair``2[TKey,TValue][]"}
        )
    }

    It "Serializes the name of type <InputType> to <ExpectedName>" -TestCases $typeNameTestCases {
        param([type]$InputType, [string]$ExpectedName)

        $name = [Microsoft.PowerShell.CrossCompatibility.Utility.TypeDataConversion]::GetFullTypeName($InputType)
        $name | Should -BeExactly $ExpectedName
    }

    It "Null type gives null type name" {
        [Microsoft.PowerShell.CrossCompatibility.Utility.TypeDataConversion]::GetFullTypeName($null) | Should -Be $null
    }
}

Describe "PowerShell version object" {
    Context "Version parsing" {
        BeforeAll {
            $genericVerCases = @(
                @{ VerStr = '6'; Major = 6; Minor = 0; Patch = 0 }
                @{ VerStr = '6.1'; Major = 6; Minor = 1; Patch = 0 }
                @{ VerStr = '5.2.7'; Major = 5; Minor = 2; Patch = 7 }
                @{ VerStr = '512.2124.71'; Major = 512; Minor = 2124; Patch = 71 }
            )

            $semVerCases = @(
                @{ VerStr = '6.1.0-rc.1'; Major = 6; Minor = 1; Patch = 0; Label = 'rc.1' }
                @{ VerStr = '6-preview.2'; Major = 6; Minor = 0; Patch = 0; Label = 'preview.2' }
                @{ VerStr = '6.2-preview.2'; Major = 6; Minor = 2; Patch = 0; Label = 'preview.2' }
            )

            $systemVerCases = @(
                @{ VerStr = '5.2.1.12312'; Major = 5; Minor = 2; Patch = 1; Revision = 12312 }
            )

            $versionFailCases = @(
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
            )
        }

        It "Parses version string '<VerStr>' as <Major>.<Minor>.<Patch>" -TestCases $semVerCases {
            param([string]$VerStr, [int]$Major, [int]$Minor, [int]$Patch)

            $v = [Microsoft.PowerShell.CrossCompatibility.Utility.PowerShellVersion]::Parse($VerStr)

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
        }

        It "Parses version string '<VerStr>' as <Major>.<Minor>.<Patch>-<Label>" -TestCases $semVerCases {
            param([string]$VerStr, [int]$Major, [int]$Minor, [int]$Patch, [string]$Label)

            $v = [Microsoft.PowerShell.CrossCompatibility.Utility.PowerShellVersion]::Parse($VerStr)

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
            $v.PreReleaseLabel | Should -BeExactly $Label
        }

        It "Parses version string '<VerStr>' as <Major>.<Minor>.<Patch>.<Revision>" -TestCases $systemVerCases {
            param([string]$VerStr, [int]$Major, [int]$Minor, [int]$Patch, [int]$Revision)

            $v = [Microsoft.PowerShell.CrossCompatibility.Utility.PowerShellVersion]::Parse($VerStr)

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
            $v.Revision | Should -Be $Revision
        }

        It "Does not parse '<VerStr>' as a version" -TestCases $versionFailCases {
            param([string]$VerStr)

            { [Microsoft.PowerShell.CrossCompatibility.Utility.PowerShellVersion]::Parse($VerStr) } | Should -Throw
        }
    }

    Context "Version creation from other versions" {
        BeforeAll {
            $versionCreationTests = @(
                @{ Version = '6.1'; Major = 6; Minor = 1; Patch = 0 }
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
                )
            }

            $versionCreationFailTests = @(
                @{ Version = $null }
                @{ Version = New-Object 'object' }
                @{ Version = 'Hello' }
            )
        }

        It "Creates a PowerShellVersion from '<Version>'" -TestCases $versionCreationTests {
            param($Version, [int]$Major, [int]$Minor, [int]$Patch, [int]$Revision, $Label)

            $v = [Microsoft.PowerShell.CrossCompatibility.Utility.PowerShellVersion]::Create($Version)

            $v.Major | Should -Be $Major
            $v.Minor | Should -Be $Minor
            $v.Patch | Should -Be $Patch
            $v.PreReleaseLabel | Should -Be $Label

            if ($Revision)
            {
                $v.Revision | Should -Be $Revision
            }
        }

        It "Does not create a PowerShellVersion from <Version>" -TestCases $versionCreationFailTests {
            param($Version)

            { [Microsoft.PowerShell.CrossCompatibility.Utility.PowerShellVersion]::Create($Version) } | Should -Throw
        }
    }
}