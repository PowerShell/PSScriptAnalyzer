# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Evaluated at discovery time because -Skip is bound during discovery.
# File system case rules differ per platform and even per volume, so they are probed rather than
# assumed from the OS:
#   Windows: case-insensitive, case-preserving; '\' and '/' both separate path segments.
#   Linux:   usually case-sensitive; 'Helpers.ps1' and 'helpers.ps1' can coexist, and PowerShell
#            folds '\' to '/' in FileSystem paths.
#   macOS:   usually case-insensitive and case-preserving, but case-sensitive volumes exist.
# Tests that need two files differing only by case are skipped on case-insensitive file systems.
# A case-mismatched reference is deliberately folded onto the file it names instead of being judged
# per file system; PSUseCorrectCasingForDotSourcedFiles reports that mismatch.
function Test-CaseSensitiveFileSystem {
    $probeDirectory = Join-Path ([IO.Path]::GetTempPath()) ([IO.Path]::GetRandomFileName())
    $null = New-Item -ItemType Directory -Path $probeDirectory
    try {
        $null = New-Item -ItemType File -Path (Join-Path $probeDirectory 'probe.txt')
        return -not (Test-Path -LiteralPath (Join-Path $probeDirectory 'PROBE.TXT'))
    }
    finally {
        Remove-Item -LiteralPath $probeDirectory -Recurse -Force
    }
}
$caseSensitive = Test-CaseSensitiveFileSystem

Describe "Functions reach across the files of a dot-sourced group" {
    BeforeAll {
        if (-not ('DotSourceScopeOutputWriter' -as [type])) {
            Add-Type -Language CSharp -IgnoreWarnings -ReferencedAssemblies @(
                [Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer].Assembly.Location,
                [System.Management.Automation.ErrorRecord].Assembly.Location) @"
        using System.Management.Automation;
        using Microsoft.Windows.PowerShell.ScriptAnalyzer;

        public class DotSourceScopeOutputWriter : IOutputWriter
        {
            public void WriteError(ErrorRecord error) { }
            public void WriteWarning(string message) { }
            public void WriteVerbose(string message) { }
            public void WriteDebug(string message) { }
            public void ThrowTerminatingError(ErrorRecord record)
            {
                throw new RuntimeException("Terminating error: " + record.ToString(), null, record);
            }
        }
"@
        }
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules        = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }

        function NewWorkload {
            param([hashtable]$Files)

            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            foreach ($relativePath in $Files.Keys) {
                $fullPath = Join-Path $root $relativePath
                $null = New-Item -Path (Split-Path $fullPath -Parent) -ItemType Directory -Force
                Set-Content -Path $fullPath -Value $Files[$relativePath] -Encoding utf8
            }
            return $root
        }
    }

    Context "Files sharing a root" {
        It "sees a function defined by a file the same root dot-sourced" {
            # Library files call each other without dot-sourcing; they share a session state only
            # because the root sourced them both.
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Caller.ps1'     = "Get-ChildItem -PATH 'x'"
                'Root.psm1'          = ". `$PSScriptRoot/lib/Definition.ps1`n. `$PSScriptRoot/lib/Caller.ps1"
            }

            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "follows a chain of dot-sources" {
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Middle.ps1'     = ". `$PSScriptRoot/Definition.ps1"
                'lib/Caller.ps1'     = "Get-ChildItem -PATH 'x'"
                'Root.psm1'          = ". `$PSScriptRoot/lib/Middle.ps1`n. `$PSScriptRoot/lib/Caller.ps1"
            }

            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }
    }

    Context "Files that only share a dependency" {
        It "does not see a function from another root that happens to share a dot-sourced file" {
            # Both roots source Shared.ps1, but that does not put the first root's functions in
            # the second's session state. Merging the two groups would hide this diagnostic.
            $root = NewWorkload @{
                'lib/Shared.ps1'     = 'function Get-Shared { }'
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'RootOne.ps1'        = ". `$PSScriptRoot/lib/Shared.ps1`n. `$PSScriptRoot/lib/Definition.ps1"
                'RootTwo.ps1'        = ". `$PSScriptRoot/lib/Shared.ps1`nGet-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not see a function from an unrelated file in the same run" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Unrelated.ps1'  = "Get-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not carry scope into a later single-file analysis" {
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Caller.ps1'     = "Get-ChildItem -PATH 'x'"
                'Root.psm1'          = ". `$PSScriptRoot/lib/Definition.ps1`n. `$PSScriptRoot/lib/Caller.ps1"
            }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty

            $caller = Join-Path $root 'lib/Caller.ps1'
            $diagnostics = @(Invoke-ScriptAnalyzer -Path $caller -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }
    }

    Context "Case-sensitive filesystems" {
        It "resolves a reference to the exact-case file when a differently-cased sibling exists" -Skip:(-not $caseSensitive) {
            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory -Force
            Set-Content -LiteralPath (Join-Path $root 'Helpers.ps1') -Value 'function Get-ChildItem { param($PATH) }'
            Set-Content -LiteralPath (Join-Path $root 'helpers.ps1') -Value 'function Other {}'
            Set-Content -LiteralPath (Join-Path $root 'Caller.ps1') -Value ". `$PSScriptRoot/helpers.ps1`nGet-ChildItem -PATH x"
            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'Caller.ps1')
        }
        It "does not collapse files that differ only by case" -Skip:(-not $IsLinux) {
            # Regression test: on a case-sensitive filesystem (e.g. Linux ext4), 'A.ps1' and 'a.ps1'
            # are distinct files. If they were folded into a single dictionary entry (case-insensitively),
            # 'a.ps1' could incorrectly inherit 'A.ps1's function definitions even though it never
            # dot-sources it, and its own call would go unreported.
            # PowerShell hashtable literals are case-insensitive, so the two files are written directly
            # rather than through NewWorkload's [hashtable]$Files parameter.
            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory -Force
            Set-Content -Path (Join-Path $root 'A.ps1') -Value 'function Get-ChildItem { param($PATH) }' -Encoding utf8
            Set-Content -Path (Join-Path $root 'a.ps1') -Value "Get-ChildItem -PATH 'x'" -Encoding utf8

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'a.ps1')
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }
    }

    Context "Dot-source targets that cannot be resolved" {
        It "ignores a target built from a variable" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = ". `$someFile`nGet-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "ignores a target outside the analyzed set" {
            $root = NewWorkload @{
                'Caller.ps1' = ". `$PSScriptRoot/DoesNotExist.ps1`nGet-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not loop on a dot-source cycle" {
            $root = NewWorkload @{
                'First.ps1'  = ". `$PSScriptRoot/Second.ps1`nfunction Get-ChildItem { param(`$PATH) }"
                'Second.ps1' = ". `$PSScriptRoot/First.ps1`nGet-ChildItem -PATH 'x'"
            }

            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }
    }

    Context "Dot-sources inside a child scope" {
        It "does not export a definition dot-sourced inside <Name> into the file scope" -TestCases @(
            @{ Name = 'a function'; Caller = 'function Load { . $PSScriptRoot/Definition.ps1 }' }
            @{ Name = 'an invoked script block'; Caller = '& { . $PSScriptRoot/Definition.ps1 }' }
        ) {
            param($Caller)
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = "$Caller`nGet-ChildItem -PATH x"
            }
            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'Caller.ps1')
        }

        It "lets a call inside the same child scope see the definition" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = 'function Load { . $PSScriptRoot/Definition.ps1; Get-ChildItem -PATH x }'
            }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "treats a dot-sourced script block as running in the caller scope" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = ". { . `$PSScriptRoot/Definition.ps1 }`nGet-ChildItem -PATH x"
            }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "does not export a stored script block declaration into the dot-source group" {
            $root = NewWorkload @{
                'Definition.ps1' = '$unused = { function Get-ChildItem { param($PATH) } }'
                'Caller.ps1'     = "Get-ChildItem -PATH x"
                'Root.psm1'      = ". `$PSScriptRoot/Definition.ps1`n. `$PSScriptRoot/Caller.ps1"
            }
            @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings).Count | Should -Be 1
        }
    }

    Context "Separators and casing of dot-source targets" {
        # PowerShell folds '\' to '/' for FileSystem paths on Linux and macOS, so these resolve everywhere.
        It "joins the dot-source group for <Reference>" -TestCases @(
            @{ Reference = '$PSScriptRoot\lib\Definition.ps1' }
            @{ Reference = '$PSScriptRoot/lib\Definition.ps1' }
        ) {
            param($Reference)
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Caller.ps1'     = "Get-ChildItem -PATH x"
                'Root.psm1'          = ". $Reference`n. `$PSScriptRoot/lib/Caller.ps1"
            }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }

        # A mismatched reference is folded onto the real file rather than guessed per file system;
        # UseCorrectCasingForDotSourcedFiles reports the mismatch.
        It "treats a case-mismatched target as the file it names" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = ". `$PSScriptRoot/definition.ps1`nGet-ChildItem -PATH x"
            }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
            $rule = @(Invoke-ScriptAnalyzer -Path (Join-Path $root 'Caller.ps1') -Settings @{
                IncludeRules = @('PSUseCorrectCasingForDotSourcedFiles')
                Rules        = @{ PSUseCorrectCasingForDotSourcedFiles = @{ Enable = $true } }
            })
            $rule.Count | Should -Be 1
        }
    }

    Context "Scope lifetime" {
        It "does not leak into a later AnalyzeSyntaxTree call after path analysis ends by <Ending>" -TestCases @(
            @{ Ending = 'completion' }
            @{ Ending = 'an exception' }
            @{ Ending = 'early disposal' }
        ) {
            param($Ending)
            $root = NewWorkload @{
                'Bad.ps1'        = "get-childitem x"
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = "get-childitem x"
                'Root.psm1'      = ". `$PSScriptRoot/Definition.ps1`n. `$PSScriptRoot/Caller.ps1"
            }
            $caller = Join-Path $root 'Caller.ps1'
            $analyzerRunspace = [runspacefactory]::CreateRunspace()
            $analyzerRunspace.Open()
            $analyzer = New-Object Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer
            $analyzer.Initialize($analyzerRunspace, (New-Object DotSourceScopeOutputWriter), $null, [string[]]@('PSUseCorrectCasing'), $null, $null, $true, $false, $false)
            # The runspace overload resets the helper, so the opt-in rule is configured afterwards.
            $ruleOptions = New-Object 'System.Collections.Generic.Dictionary[string,object]'
            $ruleOptions['Enable'] = $true
            @($analyzer.ScriptRules).Where({ $_.GetName() -eq 'PSUseCorrectCasing' })[0].ConfigureRule($ruleOptions)
            try {
                switch ($Ending) {
                    'completion' {
                        $shouldProcess = [Func[string, string, bool]] { $true }
                        $null = @($analyzer.AnalyzePath($root, $shouldProcess, $true))
                    }
                    'an exception' {
                        $calls = 0
                        $shouldProcess = [Func[string, string, bool]] { $script:calls++; if ($script:calls -ge 3) { throw 'stop' }; $true }
                        { @($analyzer.AnalyzePath($root, $shouldProcess, $true)) } | Should -Throw
                    }
                    'early disposal' {
                        $shouldProcess = [Func[string, string, bool]] { $true }
                        $enumerator = ([System.Collections.Generic.IEnumerable[Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord]]$analyzer.AnalyzePath($root, $shouldProcess, $true)).GetEnumerator()
                        $null = $enumerator.MoveNext()
                        $enumerator.Dispose()
                    }
                }

                $tokens = $null; $errors = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput('get-childitem x', [ref]$tokens, [ref]$errors)
                @($analyzer.AnalyzeSyntaxTree($ast, $tokens, $caller)).Count | Should -Be 1
            }
            finally {
                $analyzer.CleanUp()
            }
        }
    }

    Context "Files rewritten during the run" {
        It "analyzes the fixed content rather than the content of the pre-pass" {
            $root = NewWorkload @{
                'Fixable.ps1' = 'gci'
            }

            $aliasSettings = @{ IncludeRules = @('PSAvoidUsingCmdletAliases') }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $aliasSettings -Fix | Should -BeNullOrEmpty
            (Get-Content (Join-Path $root 'Fixable.ps1') -Raw).Trim() | Should -BeExactly 'Get-ChildItem'
        }
    }
}
