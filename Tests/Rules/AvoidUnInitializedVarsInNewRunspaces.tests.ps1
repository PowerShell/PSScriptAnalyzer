$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

$ruleName = "PSAvoidUnInitializedVarsInNewRunspaces"

$settings = @{
    IncludeRules = @($ruleName)
}

Describe "AvoidUnInitializedVarsInNewRunspaces" {
    Context "Should detect something" {
        $testCases = @(
            @{
                Description = "Foreach-Object -Parallel with undeclared var"
                ScriptBlock = '{
                    1..2 | ForEach-Object -Parallel { $var }
                }'
            }
            @{
                Description = "alias foreach -parallel with undeclared var"
                ScriptBlock = '{
                    1..2 | ForEach -Parallel { $var }
                }'
            }
            @{
                Description = "alias % -parallel with undeclared var"
                ScriptBlock = '{
                    1..2 | % -Parallel { $var }
                }'
            }
            @{
                Description = "abbreviated param Foreach-Object -pa with undeclared var"
                ScriptBlock = '{
                    1..2 | foreach-object -pa { $var }
                }'
            }
            @{
                Description = "Nested Foreach-Object -Parallel with undeclared var"
                ScriptBlock = '{
                    $myNestedScriptBlock = {
                        1..2 | ForEach-Object -Parallel { $var }
                    }
                }'
            }
        )

        it "should emit for: <Description>" -TestCases $testCases {
            param($Description, $ScriptBlock)
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptBlock -Settings $settings
            $warnings.Count | Should -Be 1
        }
    }

    Context "Should not detect anything" {
        $testCases = @(
            @{
                Description = "Foreach-Object with uninitialized var inside"
                ScriptBlock = '{
                    1..2 | ForEach-Object { $var }
                }'
            }
            @{
                Description = "Foreach-Object -Parallel with uninitialized `$using: var"
                ScriptBlock = '{
                    1..2 | foreach-object -Parallel { $using:var }
                }'
            }
            @{
                Description = "Foreach-Object -Parallel with var assigned locally"
                ScriptBlock = '{
                    1..2 | ForEach-Object -Parallel { $var="somevalue" }
                }'
            }
            @{
                Description = "Foreach-Object -Parallel with built-in var '`$Args' inside"
                ScriptBlock = '{
                    1..2 | ForEach-Object { $Args[0] } -ArgumentList "a" -Parallel
                }'
            }
        )

        it "should not emit anything for: <Description>" -TestCases $testCases {
            param($Description, $ScriptBlock)
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptBlock -Settings $settings
            $warnings.Count | Should -Be 0
        }
    }
}