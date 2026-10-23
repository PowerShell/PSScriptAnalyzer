# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Covers the review comments on PowerShell/PSScriptAnalyzer#2206 (review 5621413757).
# File-system specific cases probe the real file system rather than assuming a rule per OS:
# Windows is case-insensitive with '\' as a separator; Linux is usually case-sensitive and PowerShell
# folds '\' to '/' there; macOS is usually case-insensitive/case-preserving but can be case-sensitive.
# This is evaluated at discovery time because -Skip is bound during discovery.
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
$onWindows = ($PSVersionTable.PSEdition -eq 'Desktop') -or $IsWindows

Describe 'PR 2206 review follow-ups' {
    BeforeAll {
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules        = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }

        function NewWorkload {
            param([hashtable]$Files)
            $root = Join-Path $TestDrive ([IO.Path]::GetRandomFileName())
            foreach ($relativePath in $Files.Keys) {
                $fullPath = Join-Path $root $relativePath
                $null = New-Item -Path (Split-Path $fullPath -Parent) -ItemType Directory -Force
                Set-Content -LiteralPath $fullPath -Value $Files[$relativePath] -Encoding utf8
            }
            return $root
        }

        # The wrong-case parameter in 'Get-ChildItem -PATH' is reported only when the call binds to
        # the real cmdlet, so the number of diagnostics shows whether a local definition was assumed.
        function GetCasingDiagnostics {
            param($Root)
            @(Invoke-ScriptAnalyzer -Path $Root -Recurse -Settings $casingSettings)
        }

        if (-not ('ReviewFollowUpOutputWriter' -as [type])) {
            Add-Type -Language CSharp -IgnoreWarnings -ReferencedAssemblies @(
                [Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer].Assembly.Location,
                [System.Management.Automation.ErrorRecord].Assembly.Location) @"
        using System.Management.Automation;
        using Microsoft.Windows.PowerShell.ScriptAnalyzer;

        public class ReviewFollowUpOutputWriter : IOutputWriter
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
    }

    Context 'Public API compatibility' {
        It 'keeps <Type>.GetCommandInfo(string, CommandTypes?, bool)' -TestCases @(
            @{ Type = 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper' }
        ) {
            param($Type)
            $assembly = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands.InvokeScriptAnalyzerCommand].Assembly
            $signature = [Type[]]@([string], [Nullable[System.Management.Automation.CommandTypes]], [bool])
            $assembly.GetType($Type).GetMethod('GetCommandInfo', $signature) | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Alias precedence over a same-named local function' {
        It 'still resolves the alias that PowerShell prefers over the function' {
            # PowerShell resolves Alias before Function, so 'gci' is Get-ChildItem here.
            $root = NewWorkload @{ 'Script.ps1' = "function gci { }`ngci -literalpath ." }
            $diagnostics = GetCasingDiagnostics $root
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'LiteralPath'
        }

        It 'honors a local function that is not shadowed by an alias' {
            $root = NewWorkload @{ 'Script.ps1' = "function Get-ChildItem { param(`$PATH) }`nGet-ChildItem -PATH x" }
            GetCasingDiagnostics $root | Should -BeNullOrEmpty
        }
    }

    Context 'Dot-sources inside a child scope' {
        It 'does not export a definition dot-sourced inside a function into the file scope' {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = "function Load { . `$PSScriptRoot/Definition.ps1 }`nGet-ChildItem -PATH x"
            }
            $diagnostics = GetCasingDiagnostics $root
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'Caller.ps1')
        }

        It 'does not export a definition dot-sourced inside an invoked script block into the file scope' {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = "& { . `$PSScriptRoot/Definition.ps1 }`nGet-ChildItem -PATH x"
            }
            (GetCasingDiagnostics $root).Count | Should -Be 1
        }

        It 'still lets a call inside the same child scope see the definition' {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = "function Load { . `$PSScriptRoot/Definition.ps1; Get-ChildItem -PATH x }"
            }
            GetCasingDiagnostics $root | Should -BeNullOrEmpty
        }

        It 'treats a dot-sourced script block as running in the caller scope' {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = ". { . `$PSScriptRoot/Definition.ps1 }`nGet-ChildItem -PATH x"
            }
            GetCasingDiagnostics $root | Should -BeNullOrEmpty
        }
    }

    Context 'Backslash separators in dot-source targets' {
        # PowerShell folds '\' to '/' for FileSystem paths on Linux and macOS, so these are valid everywhere.
        It 'joins the dot-source group for <Reference>' -TestCases @(
            @{ Reference = '$PSScriptRoot\lib\Definition.ps1' }
            @{ Reference = '$PSScriptRoot/lib\Definition.ps1' }
            @{ Reference = '.\lib\Definition.ps1' }
        ) {
            param($Reference)
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Caller.ps1'     = "Get-ChildItem -PATH x"
                'Root.psm1'          = ". $Reference`n. `$PSScriptRoot/lib/Caller.ps1"
            }
            # Relative targets resolve against the process location in PowerShell; only the
            # $PSScriptRoot forms are statically resolvable, so the relative one must stay unresolved.
            $diagnostics = GetCasingDiagnostics $root
            if ($Reference.StartsWith('$PSScriptRoot')) { $diagnostics | Should -BeNullOrEmpty }
        }
    }

    Context 'Case-mismatched dot-source targets (documented design)' {
        # Not resolved by guessing the file system's case rules: a mismatched reference is folded onto
        # the real file, and UseCorrectCasingForDotSourcedFiles reports the mismatch instead.
        It 'treats a case-mismatched target as the file it names and leaves the mismatch to the casing rule' {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = ". `$PSScriptRoot/definition.ps1`nGet-ChildItem -PATH x"
            }
            GetCasingDiagnostics $root | Should -BeNullOrEmpty
            $rule = @(Invoke-ScriptAnalyzer -Path (Join-Path $root 'Caller.ps1') -Settings @{
                IncludeRules = @('PSUseCorrectCasingForDotSourcedFiles')
                Rules        = @{ PSUseCorrectCasingForDotSourcedFiles = @{ Enable = $true } }
            })
            $rule.Count | Should -Be 1
        }
    }

    Context 'Export-ModuleMember lookup' {
        It 'is not cancelled by an unrelated nested declaration of the same name' {
            $root = NewWorkload @{
                'Module.psm1' = "function Outer { function Export-ModuleMember {} }`nfunction Test-Example { }`nExport-ModuleMember -Function Test-Example"
            }
            $rules = @(Invoke-ScriptAnalyzer -Path $root -Recurse -IncludeRule PSProvideCommentHelp)
            $rules.Count | Should -Be 1
            $rules[0].Message | Should -Match 'Test-Example'
        }
    }

    Context 'Stored script blocks' {
        It 'does not treat a declaration in a stored script block as in scope' {
            $root = NewWorkload @{ 'Script.ps1' = "`$unused = { function Get-ChildItem { param(`$PATH) } }`nGet-ChildItem -PATH x" }
            (GetCasingDiagnostics $root).Count | Should -Be 1
        }

        It 'does not treat a declaration in a script block stored in a hashtable as in scope' {
            $root = NewWorkload @{ 'Script.ps1' = "`$h = @{ Run = { function Get-ChildItem { param(`$PATH) } } }`nGet-ChildItem -PATH x" }
            (GetCasingDiagnostics $root).Count | Should -Be 1
        }

        It 'does not export a stored script block declaration into the dot-source group' {
            $root = NewWorkload @{
                'Definition.ps1' = '$unused = { function Get-ChildItem { param($PATH) } }'
                'Caller.ps1'     = "Get-ChildItem -PATH x"
                'Root.psm1'      = ". `$PSScriptRoot/Definition.ps1`n. `$PSScriptRoot/Caller.ps1"
            }
            (GetCasingDiagnostics $root).Count | Should -Be 1
        }

        It 'keeps a declaration in a command-argument script block in scope' -TestCases @(
            @{ Code = '1 | ForEach-Object { function Get-ChildItem { param($PATH) } }' }
            @{ Code = '. { function Get-ChildItem { param($PATH) } }' }
        ) {
            param($Code)
            $root = NewWorkload @{ 'Script.ps1' = "$Code`nGet-ChildItem -PATH x" }
            GetCasingDiagnostics $root | Should -BeNullOrEmpty
        }

        It 'does not leak a declaration from a directly invoked (child scope) script block' {
            $root = NewWorkload @{ 'Script.ps1' = "& { function Get-ChildItem { param(`$PATH) } }`nGet-ChildItem -PATH x" }
            (GetCasingDiagnostics $root).Count | Should -Be 1
        }
    }

    Context 'Dot-source scope lifetime' {
        It 'does not leak into a later AnalyzeSyntaxTree call after path analysis ends by <Ending>' -TestCases @(
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
            $runspace = [runspace]::DefaultRunspace
            $analyzerRunspace = [runspacefactory]::CreateRunspace()
            $analyzerRunspace.Open()
            $analyzer = New-Object Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer
            $analyzer.Initialize($analyzerRunspace, (New-Object ReviewFollowUpOutputWriter), $null, [string[]]@('PSUseCorrectCasing'), $null, $null, $true, $false, $false)
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

    Context 'Case-sensitive file system behavior' {
        It 'does not report a correctly spelled reference when a differently-cased sibling exists' -Skip:(-not $caseSensitive) {
            $root = Join-Path $TestDrive ([IO.Path]::GetRandomFileName())
            $null = New-Item -ItemType Directory -Path $root
            Set-Content -LiteralPath (Join-Path $root 'Helpers.ps1') -Value 'function A {}'
            Set-Content -LiteralPath (Join-Path $root 'helpers.ps1') -Value 'function B {}'
            Set-Content -LiteralPath (Join-Path $root 'Caller.ps1') -Value '. $PSScriptRoot/helpers.ps1'
            Invoke-ScriptAnalyzer -Path (Join-Path $root 'Caller.ps1') -IncludeRule PSUseCorrectCasingForDotSourcedFiles | Should -BeNullOrEmpty
        }

        It 'resolves a reference to the exact-case file when a differently-cased sibling exists' -Skip:(-not $caseSensitive) {
            $root = Join-Path $TestDrive ([IO.Path]::GetRandomFileName())
            $null = New-Item -ItemType Directory -Path $root
            Set-Content -LiteralPath (Join-Path $root 'Helpers.ps1') -Value 'function Get-ChildItem { param($PATH) }'
            Set-Content -LiteralPath (Join-Path $root 'helpers.ps1') -Value 'function Other {}'
            Set-Content -LiteralPath (Join-Path $root 'Caller.ps1') -Value ". `$PSScriptRoot/helpers.ps1`nGet-ChildItem -PATH x"
            $diagnostics = GetCasingDiagnostics $root
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'Caller.ps1')
        }
    }
}
