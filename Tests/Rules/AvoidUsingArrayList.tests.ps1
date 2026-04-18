# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

using namespace System.Management.Automation.Language

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
param()

BeforeAll {
    $ruleName = "PSAvoidUsingArrayList"
    $ruleMessage = "The ArrayList class is used in '{0}'. Consider using a generic collection or a fixed array instead."
    $usingCollections = 'using namespace system.collections' + [Environment]::NewLine
    $usingGeneric = 'using namespace System.Collections.Generic' + [Environment]::NewLine
}

Describe "AvoidArrayList" {

    Context "When there are violations" {

        It "Unquoted New-Object type" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object ArrayList
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object ArrayList}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object ArrayList})
        }

        It "Single quoted New-Object type" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object 'ArrayList'
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object 'ArrayList'}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object 'ArrayList'})
        }

        It "Double quoted New-Object type" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object "ArrayList"
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object "ArrayList"}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object "ArrayList"})
        }

        It "New-Object with full parameter name" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object -TypeName ArrayList
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object -TypeName ArrayList}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object -TypeName ArrayList})
        }

        It "New-Object with abbreviated parameter name and odd casing" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object -Type ArrayLIST
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object -Type ArrayLIST}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object -Type ArrayLIST})
        }

        It "New-Object with full type name" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object -TypeName System.Collections.ArrayList
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object -TypeName System.Collections.ArrayList}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object -TypeName System.Collections.ArrayList})
        }

        It "New-Object with semi full type name and odd casing" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object COLLECTIONS.ArrayList
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object COLLECTIONS.ArrayList}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object COLLECTIONS.ArrayList})
        }

        It "Type initializer with 3 parameters" {
            $scriptDefinition = $usingCollections + {
                $List = [ArrayList](1,2,3)
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {[ArrayList](1,2,3)}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {[ArrayList](1,2,3)})
        }

        It "Type initializer with array parameters" {
            $scriptDefinition = $usingCollections + {
                $List = [ArrayList]@(1,2,3)
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {[ArrayList]@(1,2,3)}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {[ArrayList]@(1,2,3)})
        }

        It "Type initializer with new constructor" {
            $scriptDefinition = $usingCollections + {
                $List = [ArrayList]::new()
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {[ArrayList]::new()}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {[ArrayList]::new()})
        }

        It "Full type name initializer with new constructor" {
            $scriptDefinition = $usingCollections + {
                $List = [System.Collections.ArrayList]::new()
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {[System.Collections.ArrayList]::new()}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {[System.Collections.ArrayList]::new()})
        }

        It "Semi full type name initializer with new constructor and odd casing" {
            $scriptDefinition = $usingCollections + {
                $List = [COLLECTIONS.ArrayList]::new()
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {[COLLECTIONS.ArrayList]::new()}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {[COLLECTIONS.ArrayList]::new()})
        }
    }

    Context "When there are no violations" {

        It "New-Object List[Object]" {
            $scriptDefinition = {
                $List = New-Object List[Object]
                1..3 | ForEach-Object { $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "[List[Object]]::new()" {
            $scriptDefinition = {
                $List = [List[Object]]::new()
                1..3 | ForEach-Object { $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Using the pipeline" {
            $scriptDefinition = {
                $List = 1..3 | ForEach-Object { $_ }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Out of the namespace scope" {
            $scriptDefinition = $usingGeneric + {
                $List = New-Object ArrayList
                $List = [ArrayList](1,2,3)
                $List = [ArrayList]@(1,2,3)
                $List = [ArrayList]::new()
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Disabled" {

        BeforeAll {
            $settings = @{
                IncludeRules = @($ruleName)
                Rules        = @{ $ruleName = @{ Enable = $false } }
            }
        }

        It "New-Object type" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object ArrayList
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It "Type initializer" {
            $scriptDefinition = $usingCollections + {
                $List = [ArrayList](1,2,3)
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It "New constructor" {
            $scriptDefinition = $usingCollections + {
                $List = [ArrayList]::new()
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Explicitly enabled" {

        BeforeAll {
            $settings = @{
                IncludeRules = @($ruleName)
                Rules        = @{ $ruleName = @{ Enable = $true } }
            }
        }

        It "New-Object type" {
            $scriptDefinition = $usingCollections + {
                $List = New-Object ArrayList
                1..3 | ForEach-Object { $null = $List.Add($_) }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Object ArrayList}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f {New-Object ArrayList})
        }
    }

    Context "Test for potential errors" {

        It "Dynamic types shouldn't error"  {
            $scriptDefinition = {
                $type = "System.Collections.ArrayList"
                New-Object -TypeName '$type'
            }.ToString()

            $analyzer = { Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName) }
            $analyzer | Should -Not -Throw # but won't violate either (too complex to cover)
        }
    }
}
