# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = 'PSUseConsistentParameterSetName'
}

Describe "UseConsistentParameterSetName" {
    Context "When there are case mismatch violations between DefaultParameterSetName and ParameterSetName" {
        It "detects case mismatch between DefaultParameterSetName and ParameterSetName" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='SetOne')]
    param(
        [Parameter(ParameterSetName='setone')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='SetTwo')]
        [string]$Parameter2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Severity | Should -Be 'Warning'
            $violations[0].Message | Should -Match "DefaultParameterSetName 'SetOne' does not match the case of ParameterSetName 'setone'"
        }

        It "detects multiple case mismatches with DefaultParameterSetName" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='SetOne')]
    param(
        [Parameter(ParameterSetName='setone')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='SETONE')]
        [string]$Parameter2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 2
            $violations | ForEach-Object { $_.Severity | Should -Be 'Warning' }
        }
    }

    Context "When there are case mismatch violations between ParameterSetName values" {
        It "detects case mismatch between different ParameterSetName values" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='SetOne')]
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='setone')]
        [string]$Parameter2,

        [Parameter(ParameterSetName='SetTwo')]
        [string]$Parameter3
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Severity | Should -Be 'Warning'
            $violations[0].Message | Should -Match "ParameterSetName 'setone' does not match the case of 'SetOne'"
        }

        It "detects multiple case variations of the same parameter set name" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='setone')]
        [string]$Parameter2,

        [Parameter(ParameterSetName='SETONE')]
        [string]$Parameter3
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 2  # Two mismatches with the first occurrence
            $violations | ForEach-Object { $_.Severity | Should -Be 'Warning' }
        }
    }

    Context "When DefaultParameterSetName is missing" {
        It "warns when parameter sets are used but DefaultParameterSetName is not specified" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding()]
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='SetTwo')]
        [string]$Parameter2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Severity | Should -Be 'Warning'
            $violations[0].Message | Should -Match "uses parameter sets but does not specify a DefaultParameterSetName"
        }
    }

    Context "When a parameter is declared multiple times in the same parameter set" {
        It "detects duplicate parameter declarations in the same parameter set (explicit)" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName='SetOne')]
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 2
            $violations | ForEach-Object { $_.Message | Should -Be "Parameter 'Parameter1' is declared in parameter-set 'SetOne' multiple times." }
        }

        It "detects duplicate parameter declarations in the same parameter set (implicit via omitted ParameterSetName)" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter()]
        [Parameter()]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 2
            $violations | ForEach-Object { $_.Message | Should -Be "Parameter 'Parameter1' is declared in parameter-set '__AllParameterSets' multiple times." }
        }

        It "detects duplicate parameter declarations in explicit and implicit parameter sets" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName='__AllParameterSets')]
        [Parameter()]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 2
            $violations | ForEach-Object { $_.Message | Should -Be "Parameter 'Parameter1' is declared in parameter-set '__AllParameterSets' multiple times." }
        }


    }

    Context "When ParameterSetNames contain inadvisable characters" {
        It "detects ParameterSetName containing a new line" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName="Set`nOne")]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "should not contain new lines"
        }

        It "detects ParameterSetName containing a carriage return" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName="Set`rOne")]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "should not contain new lines"
        }

        It "detects ParameterSetName containing mixed newline types" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName="Set`r`nOne")]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
        }

        It "detects DefaultParameterSetName containing a new line" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName="Set`nOne")]
    param(
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "should not contain new lines"
        }

        It "detects DefaultParameterSetName containing a carriage return" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName="Set`rOne")]
    param(
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "should not contain new lines"
        }

        # Missing: DefaultParameterSetName with newlines
        It "detects DefaultParameterSetName containing mixed newline types" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName="Set`r`nOne")]
    param([string]$Parameter1)
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
        }

    }

    Context "When there are no violations" {
        It "does not flag functions without CmdletBinding" {
            $scriptDefinition = @'
function Test-Function {
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }

        It "does not flag functions without parameter sets" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding()]
    param(
        [string]$Parameter1,
        [string]$Parameter2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }

        It "does not flag when DefaultParameterSetName and ParameterSetName cases match exactly" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='SetOne')]
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='SetTwo')]
        [string]$Parameter2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }

        It "does not flag when all ParameterSetName cases match exactly" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='SetOne')]
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter2,

        [Parameter(ParameterSetName='SetTwo')]
        [string]$Parameter3
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }

        # This could be a case where the function can be run without any parameters
        # in the default set.
        It "does not flag when DefaultParameterSetName doesn't match any ParameterSetName" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='Default')]
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [Parameter(ParameterSetName='SetTwo')]
        [string]$Parameter2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }

        It "handles parameters without attributes correctly" {
            $scriptDefinition = @'
function Test-Function {
    [CmdletBinding(DefaultParameterSetName='SetOne')]
    param(
        [Parameter(ParameterSetName='SetOne')]
        [string]$Parameter1,

        [string]$CommonParameter  # No Parameter attribute
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }
    }

    Context "Real-world scenarios" {
        It "handles complex parameter set definitions correctly" {
            $scriptDefinition = @'
function Test-ComplexFunction {
    [CmdletBinding(DefaultParameterSetName='ByName')]
    param(
        [Parameter(ParameterSetName='ByName', Mandatory)]
        [string]$Name,

        [Parameter(ParameterSetName='ByName')]
        [Parameter(ParameterSetName='ByID')]
        [string]$ComputerName,

        [Parameter(ParameterSetName='ByID', Mandatory)]
        [int]$ID
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 0
        }

        It "detects case issues in complex scenarios" {
            $scriptDefinition = @'
function Test-ComplexFunction {
    [CmdletBinding(DefaultParameterSetName='ByName')]
    param(
        [Parameter(ParameterSetName='byname', Mandatory)]
        [string]$Name,

        [Parameter(ParameterSetName='ByName')]
        [Parameter(ParameterSetName='ByID')]
        [string]$ComputerName,

        [Parameter(ParameterSetName='byid', Mandatory)]
        [int]$ID
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 2  # 'byname' and 'byid' case mismatches
            $violations | ForEach-Object { $_.Severity | Should -Be 'Warning' }
        }
    }
}
