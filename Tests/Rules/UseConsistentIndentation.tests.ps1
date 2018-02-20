$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

$indentationUnit = ' '
$indentationSize = 4
$ruleConfiguration = @{
    Enable          = $true
    IndentationSize = 4
    Kind            = 'space'
}

$settings = @{
    IncludeRules = @("PSUseConsistentIndentation")
    Rules        = @{
        PSUseConsistentIndentation = $ruleConfiguration
    }
}

Describe "UseConsistentIndentation" {
    Context "When top level indentation is not consistent" {
        BeforeAll {
            $def = @'
 function foo ($param1)
{

}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should detect a violation" {
            $violations.Count | Should -Be 1
        }
    }

    Context "When nested indenation is not consistent" {
        BeforeAll {
            $def = @'
function foo ($param1)
{
"abc"
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should -Be 1
        }
    }

    Context "When a multi-line hashtable is provided" {
        BeforeAll {
            $def = @'
$hashtable = @{
a = 1
b = 2
    c = 3
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find violations" {
            $violations.Count | Should -Be 2
        }
    }

    Context "When a multi-line array is provided" {
        BeforeAll {
            $def = @'
$array = @(
1,
    2,
3)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find violations" {
            $violations.Count | Should -Be 2
        }
    }

    Context "When a param block is provided" {
        BeforeAll {
            $def = @'
param(
            [string] $param1,

[string]
    $param2,

        [string]
$param3
)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find violations" {
            $violations.Count | Should -Be 4
        }
    }

    Context "When a sub-expression is provided" {
        BeforeAll {
            $def = @'
function foo {
    $x = $("abc")
    $x
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should not find a violations" {
            $violations.Count | Should -Be 0
        }
    }

    Context "When a multi-line command is given" {
        It "Should find a violation if a pipleline element is not indented correctly" {
            $def = @'
get-process |
where-object {$_.Name -match 'powershell'}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Should not find a violation if a pipleline element is indented correctly" {
            $def = @'
get-process |
    where-object {$_.Name -match 'powershell'}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }

        It "Should ignore comment in the pipleline" {
            $def = @'
  get-process |
    where-object Name -match 'powershell' | # only this is indented correctly
select Name,Id |
       format-list
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 3
        }

        It "Should indent properly after line continuation (backtick) character" {
            $def = @'
$x = "this " + `
"Should be indented properly"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = "`"Should be indented properly`""
                CorrectionText   = (New-Object -TypeName String -ArgumentList $indentationUnit, $indentationSize) + "`"Should be indented properly`""
            }
            Test-CorrectionExtentFromContent @params
        }
    }

    Context "When tabs instead of spaces are used for indentation" {
        BeforeAll {
            $ruleConfiguration.'Kind' = 'tab'
        }

        It "Should indent using tabs" {
            $def = @'
function foo
{
get-childitem
$x=1+2
$hashtable = @{
property1 = "value"
    anotherProperty = "another value"
}
}
'@
            ${t} = "`t"
            $expected = @"
function foo
{
${t}get-childitem
${t}`$x=1+2
${t}`$hashtable = @{
${t}${t}property1 = "value"
${t}${t}anotherProperty = "another value"
${t}}
}
"@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }
}
