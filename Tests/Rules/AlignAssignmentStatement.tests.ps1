# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    function New-AlignAssignmentSettings {
        [OutputType([hashtable])]
        [CmdletBinding()]
        param(
            [Parameter()]
            [bool]
            $CheckHashtable = $false,
            [Parameter()]
            [bool]
            $AlignHashtableKvpWithInterveningComment = $false,
            [Parameter()]
            [bool]
            $CheckEnums = $false,
            [Parameter()]
            [bool]
            $IncludeValuelessEnumMembers = $false,
            [Parameter()]
            [bool]
            $AlignEnumMemberWithInterveningComment = $false
        )
        return @{
            IncludeRules = @('PSAlignAssignmentStatement')
            Rules        = @{
                PSAlignAssignmentStatement = @{
                    Enable                                  = $true
                    CheckHashtable                          = $CheckHashtable
                    AlignHashtableKvpWithInterveningComment = $AlignHashtableKvpWithInterveningComment
                    CheckEnums                              = $CheckEnums
                    IncludeValuelessEnumMembers             = $IncludeValuelessEnumMembers
                    AlignEnumMemberWithInterveningComment   = $AlignEnumMemberWithInterveningComment
                }
            }
        }
    }

    function Get-NonParseDiagnostics {
        [OutputType([object[]])]
        [CmdletBinding()]
        param(
            [Parameter(Mandatory, ValueFromPipeline)]
            [object[]]
            $Diagnostics
        )
        process {
            $Diagnostics | Where-Object {
                $_.RuleName -eq 'PSAlignAssignmentStatement'
            }
        }
    }

    function Apply-Corrections {
        [OutputType([string])]
        [CmdletBinding()]
        param(
            [string]
            $Original,
            [object[]]
            $Diagnostics
        )
        # Note: This only works to apply the correction extents because all of
        # our corrections are simple, single line operations.
        $lines = $Original -split "`n"
        foreach ($Diagnostic in $Diagnostics) {
            if (-not $Diagnostic.SuggestedCorrections) {
                continue
            }
            foreach ($extent in $Diagnostic.SuggestedCorrections) {
                $lineIndex = $extent.StartLineNumber - 1
                $prefix = $lines[$lineIndex].Substring(
                    0, $extent.StartColumnNumber - 1
                )
                $suffix = $lines[$lineIndex].Substring(
                    $extent.EndColumnNumber - 1
                )
                $lines[$lineIndex] = $prefix + $extent.Text + $suffix

            }
        }
        return ($lines -join "`n")
    }
}

Describe 'AlignAssignmentStatement' {

    Context 'When checking Hashtables is disabled' {

        It 'Should not find violations in mis-aligned hashtables' {
            $def = @'
@{
    'Key' = 'Value'
    'LongerKey' = 'Value'
}
'@
            $settings = New-AlignAssignmentSettings

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty

        }

        It 'Should not find violations in DSC configuration blocks' {
            $def = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name = '"RSAT"'
        }
    }
}
'@
        $settings = New-AlignAssignmentSettings

        Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
            Get-NonParseDiagnostics |
            Should -BeNullOrEmpty

        } -Skip:($IsLinux -or $IsMacOS)

    }

    Context 'When Hashtable checking is enabled' {

        It 'Should not find violations in empty single-line hashtable' {
            $def = '@{}'

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should not find violations in empty multi-line hashtable' {
            $def = @'
@{

}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should not find violation in aligned, single-line, single-kvp hashtable' {
            $def = '@{"Key" = "Value"}'

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -BeNullOrEmpty
        }

        It 'Should find violation in mis-aligned, single-line, single-kvp hashtable' {
            $def = '@{"Key"    = "Value"}'
            $expected = '@{"Key" = "Value"}'

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations

            $corrected | Should -BeExactly $expected
        }

        It 'Should not find violations in mis-aligned hashtable with multiple kvp on a single line' {
            $def = '@{"Key1"    = "Value1";"Key2"="Value2"}'

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -BeNullOrEmpty
        }

        It 'Should not find violations in well aligned, multi-line, multi-kvp hashtable' {
            $def = @'
@{
    'Key1' = 'Value1'
    'Key2' = 'Value2'
    'Key3' = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should find violations in mis-aligned, multi-line, multi-kvp hashtable' {
            $def = @'
@{
    'Key1'= 'Value1'
    'Key12' = 'Value2'
    'Key123' = 'Value3'
}
'@

            $expected = @'
@{
    'Key1'   = 'Value1'
    'Key12'  = 'Value2'
    'Key123' = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 2

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should ignore lines with intervening comments when AlignHashtableKvpWithInterveningComment is false' {
            $def = @'
@{
    'Key1' <#comment#>= 'Value1'
    'Key12' = 'Value2'
    'Key123' = 'Value3'
}
'@

            $expected = @'
@{
    'Key1' <#comment#>= 'Value1'
    'Key12'  = 'Value2'
    'Key123' = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should align lines with intervening comments when AlignHashtableKvpWithInterveningComment is true' {
            $def = @'
@{
    'Key1' <#comment#>= 'Value1'
    'Key12' = 'Value2'
    'Key123' = 'Value3'
}
'@

            $expected = @'
@{
    'Key1' <#comment#> = 'Value1'
    'Key12'            = 'Value2'
    'Key123'           = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true -AlignHashtableKvpWithInterveningComment $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 3

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should not find violations when intervening comment is already aligned and AlignHashtableKvpWithInterveningComment is true' {
            $def = @'
@{
    'Key1' <#comment#> = 'Value1'
    'Key2'             = 'Value2'
    'Key3'             = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true -AlignHashtableKvpWithInterveningComment $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should not find violations when intervening comment is on right of equals sign and AlignHashtableKvpWithInterveningComment is true' {
            $def = @'
@{
    'Key1' = <#comment#> 'Value1'
    'Key2' = 'Value2'
    'Key3' = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true -AlignHashtableKvpWithInterveningComment $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should ignore kvp with a line continuation between key and equals sign' {
            $def = @'
@{
    'LongerKey' `
        = <#comment#> 'Value1'
    'Key1' = 'Value2'
    'Key12' = 'Value3'
}
'@

            $expected = @'
@{
    'LongerKey' `
        = <#comment#> 'Value1'
    'Key1'  = 'Value2'
    'Key12' = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should correctly align kvp when key is a string containing an equals sign' {
            $def = @'
@{
    'key1=5'   = 'Value1'
    'Key1' = 'Value2'
    'Key12' = 'Value3'
}
'@

            $expected = @'
@{
    'key1=5' = 'Value1'
    'Key1'   = 'Value2'
    'Key12'  = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 3

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should correctly align kvp when key is an expression containing an assignment' {
            # Note: `($key='key1')` defines the variable `$key` and sets it's
            #       value to 'key1'. The entire expression evaluates to 'key1'
            #       which is then used as the hashtable key. So the first key
            #       at runtime is equal to the string 'key1'.
            $def = @'
@{
    ($key='key1')   = 'Value1'
    'Key2' = 'Value2'
    'Key3' = 'Value3'
}
'@

            $expected = @'
@{
    ($key='key1') = 'Value1'
    'Key2'        = 'Value2'
    'Key3'        = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 3

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should correctly align hashtables independantly when nested' {
            $def = @'
@{
    'key1'     = 5
    'key12' = @{
        'nestedKey1'      = 'Value1'
        'nestedKey12'= 'Value2'
        'nestedKey123'= @{
            'superNestedKey1' = 'Value1'
            'superNestedKey12'='Value2'
        }
    }
    'key123'    = 'Value3'
}
'@

            $expected = @'
@{
    'key1'   = 5
    'key12'  = @{
        'nestedKey1'   = 'Value1'
        'nestedKey12'  = 'Value2'
        'nestedKey123' = @{
            'superNestedKey1'  = 'Value1'
            'superNestedKey12' ='Value2'
        }
    }
    'key123' = 'Value3'
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 8

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should not find violations in aligned DSC configuration blocks' {
            $def = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name   = '"RSAT"'
        }
    }
}
'@
            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty

        } -Skip:($IsLinux -or $IsMacOS)

        It 'Should find violations in mis-aligned DSC configuration blocks' {
            $def = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name = '"RSAT"'
        }
    }
}
'@

            $expected = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name   = '"RSAT"'
        }
    }
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected

        } -Skip:($IsLinux -or $IsMacOS)

        It 'Should ignore lines in DSC configuration blocks with intervening comments when AlignHashtableKvpWithInterveningComment is false' {
            $def = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name <#asdasd#>= '"RSAT"'
            Other = 'Value'
        }
    }
}
'@

            $expected = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name <#asdasd#>= '"RSAT"'
            Other  = 'Value'
        }
    }
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true -AlignHashtableKvpWithInterveningComment $false

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        } -Skip:($IsLinux -or $IsMacOS)

        It 'Should align lines in DSC configuration blocks with intervening comments when AlignHashtableKvpWithInterveningComment is true' {
            $def = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name <#asdasd#>= '"RSAT"'
            Other = 'Value'
        }
    }
}
'@

            $expected = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure          = '"Present"'
            Name <#asdasd#> = '"RSAT"'
            Other           = 'Value'
        }
    }
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true -AlignHashtableKvpWithInterveningComment $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 3

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        } -Skip:($IsLinux -or $IsMacOS)

        It 'Should ignore lines with a line continuation in DSC configuration blocks' {
            $def = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name `
                = '"RSAT"'
            Other = 'Value'
        }
    }
}
'@

            $expected = @'
Configuration C1 {
    Node localhost {
        NonExistentResource X {
            Ensure = '"Present"'
            Name `
                = '"RSAT"'
            Other  = 'Value'
        }
    }
}
'@

            $settings = New-AlignAssignmentSettings -CheckHashtable $true -AlignHashtableKvpWithInterveningComment $false

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        } -Skip:($IsLinux -or $IsMacOS)

    }

    Context 'When Enum checking is disabled' {

        It 'Should not find violations in mis-aligned enums' {
            $def = @'
enum E1 {
    Short = 1
    Longer = 2
    Longest = 3
}
'@
            $settings = New-AlignAssignmentSettings

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty

        }

    }

    Context 'When Enum checking is enabled' {

        It 'Should not find violations in empty single-line enum' {
            $def = 'enum E1 {}'

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should not find violations in empty multi-line enum' {
            $def = @'
enum E1 {

}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should not find violations in single-member, valueless, single-line enum' {
            $def = 'enum E1 { Member }'

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should not find violations in aligned single-member, explicitly valued, single-line enum' {
            $def = 'enum E1 { Member = 1 }'

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics |
                Should -BeNullOrEmpty
        }

        It 'Should find violations in mis-aligned single-member, explicitly valued, single-line enum' {
            $def = 'enum E1 { Member       = 1 }'

            $expected = 'enum E1 { Member = 1 }'

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should find violations in mis-aligned single-member, explicitly valued, multi-line enum' {
            $def = @'
enum E1 {
    Member       = 1
}
'@

            $expected = @'
enum E1 {
    Member = 1
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should not find violations in aligned, multi-member enum' {
            $def = @'
enum E1 {
    Member1 = 1
    Member2 = 2
    Member3 = 3
    Member4 = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -BeNullOrEmpty
        }

        It 'Should find violations in mis-aligned, multi-member enum' {
            $def = @'
enum E1 {
    Member1 = 1
    Member12 = 2
    Member123 = 3
    Member1234 = 4
}
'@

            $expected = @'
enum E1 {
    Member1    = 1
    Member12   = 2
    Member123  = 3
    Member1234 = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 3

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should find violations in mis-aligned, multi-member, mixed-valued enum' {
            $def = @'
enum E1 {
    Member1 = 1
    Member12
    Member123 = 3
    Member1234
}
'@

            $expected = @'
enum E1 {
    Member1   = 1
    Member12
    Member123 = 3
    Member1234
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 1

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should ignore lines with intervening comments when AlignEnumMemberWithInterveningComment is false' {
            $def = @'
enum E1 {
    Member1 = 1
    Member12 = 2
    Member123 <#Comment#>= 3
    Member1234 = 4
}
'@

            $expected = @'
enum E1 {
    Member1    = 1
    Member12   = 2
    Member123 <#Comment#>= 3
    Member1234 = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true -AlignEnumMemberWithInterveningComment $false

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 2

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should align lines with intervening comments when AlignHashtableKvpWithInterveningComment is true' {
            $def = @'
enum E1 {
    Member1 = 1
    Member12 = 2
    Member123 <#Comment#>= 3
    Member1234 = 4
}
'@

            $expected = @'
enum E1 {
    Member1               = 1
    Member12              = 2
    Member123 <#Comment#> = 3
    Member1234            = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true -AlignEnumMemberWithInterveningComment $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 4

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should not find violations when intervening comment is already aligned and AlignEnumMemberWithInterveningComment is true' {
            $def = @'
enum E1 {
    Member1               = 1
    Member12              = 2
    Member123 <#Comment#> = 3
    Member1234            = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true -AlignEnumMemberWithInterveningComment $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -BeNullOrEmpty
        }

        It 'Should not find violations when intervening comment is on right of equals sign and AlignEnumMemberWithInterveningComment is true' {
            $def = @'
enum E1 {
    Member1    = 1
    Member12   = 2
    Member123  = <#Comment#> 3
    Member1234 = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true -AlignEnumMemberWithInterveningComment $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -BeNullOrEmpty
        }

        It 'Should ignore member with a line continuation between name and equals sign' {
            $def = @'
enum E1 {
    Member1 = 1
    Member12 `
        = 2
    Member123 = 3
    Member1234 = 4
}
'@

            $expected = @'
enum E1 {
    Member1    = 1
    Member12 `
        = 2
    Member123  = 3
    Member1234 = 4
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 2

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should use valueless members for alignment when IncludeValuelessEnumMembers is true' {
            $def = @'
enum E1 {
    Member1 = 1
    Member12
    Member123 = 3
    Member1234
}
'@

            $expected = @'
enum E1 {
    Member1    = 1
    Member12
    Member123  = 3
    Member1234
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true -IncludeValuelessEnumMembers $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -HaveCount 2

            $corrected = Apply-Corrections -Original $def -Diagnostics $violations
            $corrected | Should -BeExactly $expected
        }

        It 'Should not find violations where all members are valueless and IncludeValuelessEnumMembers is true' {
            $def = @'
enum E1 {
    Member1
    Member12
    Member123
    Member1234
}
'@

            $settings = New-AlignAssignmentSettings -CheckEnums $true -IncludeValuelessEnumMembers $true

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings |
                Get-NonParseDiagnostics
            $violations | Should -BeNullOrEmpty
        }

    }

}