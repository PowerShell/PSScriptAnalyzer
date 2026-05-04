# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
param()

BeforeAll {
    $ruleName = "PSMissingTryBlock"
}

Describe "MissingTryBlock" {
    Context "Violates" {
        It "Catch is missing a try block" {
            $scriptDefinition = { catch { "An error occurred." } }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be catch
            $violations.Message           | Should -Be 'Catch is missing a try block'
            $violations.RuleSuppressionID | Should -Be catch
        }

        It "Finally is missing a try block" {
            $scriptDefinition = { finally { "Finalizing..." } }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be finally
            $violations.Message           | Should -Be 'Finally is missing a try block'
            $violations.RuleSuppressionID | Should -Be finally
        }

        It "Single line catch and finally is missing a try block" {
            $scriptDefinition = {
                catch { "An error occurred." } finally { "Finalizing..." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be catch
            $violations.Message           | Should -Be 'Catch is missing a try block'
            $violations.RuleSuppressionID | Should -Be catch
        }

        It "Multi line catch and finally is missing a try block" {
            $scriptDefinition = {
                catch { "An error occurred." }
                finally { "Finalizing..." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count                | Should -Be 2
            $violations[0].Severity          | Should -Be Warning
            $violations[0].Extent.Text       | Should -Be catch
            $violations[0].Message           | Should -Be 'Catch is missing a try block'
            $violations[0].RuleSuppressionID | Should -Be catch
            $violations[1].Severity          | Should -Be Warning
            $violations[1].Extent.Text       | Should -Be finally
            $violations[1].Message           | Should -Be 'Finally is missing a try block'
            $violations[1].RuleSuppressionID | Should -Be finally
        }
    }

    Context "Compliant" {
        It "try-catch block" {
            $scriptDefinition = {
                try { NonsenseString }
                catch { "An error occurred." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "try-catch-final statement" {
            $scriptDefinition = {
                try { NonsenseString }
                catch { "An error occurred." }
                finally { "Finalizing..." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Single line try statement" {
            $scriptDefinition = {
                try { NonsenseString } catch { "An error occurred." } finally { "Finalizing..." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Catch as parameter" {
            $scriptDefinition = { Write-Host Catch }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Catch as double quoted string" {
            $scriptDefinition = { "Catch" }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Catch as single quoted string" {
            $scriptDefinition = { 'Catch' }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Suppressed" {
        It "Multi line catch and finally is missing a try block" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSMissingTryBlock', '', Justification = 'Test')]
                param()
                catch { "An error occurred." }
                finally { "Finalizing..." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Multi line catch and finally is missing a try block for catch only" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSMissingTryBlock', 'finally', Justification = 'Test')]
                param()
                catch { "An error occurred." }
                finally { "Finalizing..." }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count | Should -Be 1
        }
    }
}