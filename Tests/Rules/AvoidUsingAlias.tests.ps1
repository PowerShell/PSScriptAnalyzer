# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "'cls' is an alias of 'Clear-Host'. Alias can introduce possible problems and make scripts hard to maintain. Please consider changing alias to its full content."
    $violationName = "PSAvoidUsingCmdletAliases"
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    $violationFilepath = Join-Path $PSScriptRoot 'AvoidUsingAlias.ps1'
    $violations = Invoke-ScriptAnalyzer $violationFilepath | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingAliasNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")
}

Describe "AvoidUsingAlias" {
    Context "When there are violations" {
        It "has 2 Avoid Using Alias Cmdlets violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should -Match $violationMessage
        }

        It "suggests correction" {
            Test-CorrectionExtent $violationFilepath $violations[0] 1 'iex' 'Invoke-Expression'
            $violations[0].SuggestedCorrections[0].Description | Should -Be 'Replace iex with Invoke-Expression'

            Test-CorrectionExtent $violationFilepath $violations[1] 1 'cls' 'Clear-Host'
            $violations[1].SuggestedCorrections[0].Description | Should -Be 'Replace cls with Clear-Host'
        }

        It "warns when 'Get-' prefix was omitted" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'verb' | Where-Object { $_.RuleName -eq $violationName }
            $violations.Count | Should -Be 1
        }

        It "still expands aliases with resource-shaped arguments" {
            $target = 'icm Server01 { Get-Process }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Invoke-Command'
        }

        It "still adds the Get prefix to ordinary commands with resource-shaped arguments" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'Verb Argument {}' -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }
    }

    Context "Violation Extent" {
        It "should return only the cmdlet extent" {
            $target = @'
gci -Path C:\
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName
            $violations[0].Extent.Text | Should -Be "gci"
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        It "does not add the Get prefix to script-defined functions or skip their script block arguments" {
            $target = 'function script:vErB {}; Verb Instance { ChildItem . {} }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'ChildItem'
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings |
                Should -Be ($target.Replace('ChildItem', 'Get-ChildItem'))
        }

        It "respects private function visibility" {
            $target = 'function private:Verb {}; Verb; . { Verb }; & { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings |
                Should -Be ($target.Replace('& { Verb }', '& { Get-Verb }'))
        }

        It "does not resolve function declarations before their execution" {
            $target = 'Verb; function Verb {}'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "respects execution order across caller-scoped blocks" -TestCases @(
            @{ Target = 'Verb; . { function Verb {} }' }
            @{ Target = '. { Verb }; function Verb {}' }
        ) {
            param($Target)

            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $Target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "respects execution order across ForEach-Object callback phases" -TestCases @(
            @{
                Target = '1 | ForEach-Object -Process { Verb } -Begin { function Verb {} }'
                ExpectedViolations = 0
                ExpectedText = '1 | ForEach-Object -Process { Verb } -Begin { function Verb {} }'
            }
            @{
                Target = '1 | ForEach-Object -Begin { Verb } -Process { function Verb {} }'
                ExpectedViolations = 1
                ExpectedText = '1 | ForEach-Object -Begin { Get-Verb } -Process { function Verb {} }'
            }
        ) {
            param($Target, $ExpectedViolations, $ExpectedText)

            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $Target -IncludeRule $violationName)

            $violations.Count | Should -Be $ExpectedViolations
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $Target -Settings $settings | Should -Be $ExpectedText
        }

        It "does not apply caller scope to script-block arguments of dot-invoked commands" {
            $target = 'function private:Verb {}; . Invoke-Command -ScriptBlock { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "indexes declarations in directly dot-sourced blocks in the caller scope" {
            $target = '. { function private:Verb {}; Verb }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "uses caller scope for parameterized dot-sourced blocks" {
            $target = 'function private:Verb {}; . { param($value) Verb } 1'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "recognizes caller scope for local Invoke-Command NoNewScope callbacks" {
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            $target = 'function private:Verb {}; Invoke-Command -NoNewScope -ScriptBlock { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target

            $target = 'Invoke-Command -NoNewScope -ScriptBlock { function private:Verb {}; Verb }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "does not treat a newline-separated DSC script property as caller-scoped" {
            $target = @'
function Verb {};
Script Test {
    SetScript =
    { Verb }
}
'@
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "respects caller scope in <Command> callbacks" -TestCases @(
            @{ Command = 'ForEach-Object' }
            @{ Command = 'Where-Object' }
            @{ Command = 'Microsoft.PowerShell.Core\ForEach-Object' }
            @{ Command = 'Microsoft.PowerShell.Core\Where-Object' }
        ) {
            param($Command)

            $target = "function private:Verb {}; 1 | $Command { Verb }"
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "preserves caller scope when an alias takes precedence over a same-named function" {
            $target = 'function where {}; function private:Verb {}; 1 | where { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'where'
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings |
                Should -Be ($target.Replace('1 | where', '1 | Where-Object'))
        }

        It "respects caller scope for colon-attached callback arguments" {
            foreach ($command in @(
                'ForEach-Object -Process:{ Verb }'
                'Where-Object -FilterScript:{ Verb }'
            )) {
                $target = "function private:Verb {}; 1 | $command"
                $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

                $violations.Count | Should -Be 0
                $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
                Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
            }
        }

        It "uses the bound callback parameter after switches and abbreviated parameters" {
            $target = 'function private:Verb {}; 1 | ForEach-Object -Verbose { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target

            $target = 'function private:Verb {}; 1 | ForEach-Object -Proc { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)
            $violations.Count | Should -Be 0
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "does not apply caller scope to ForEach-Object parallel callbacks" -skip:($PSVersionTable.PSVersion.Major -lt 7) {
            $target = 'function private:Verb {}; 1 | ForEach-Object -Parallel { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not find functions outside ForEach-Object parallel callbacks" -skip:($PSVersionTable.PSVersion.Major -lt 7) {
            $target = 'function Verb {}; 1 | ForEach-Object -Parallel { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not find functions outside isolated execution boundaries" -TestCases @(
            @{ Boundary = 'Start-Job { Verb }' }
            @{ Boundary = 'Invoke-Command -ComputerName localhost -ScriptBlock { Verb }' }
            @{ Boundary = 'Invoke-Command -Session $session -ScriptBlock { Verb }' }
            @{ Boundary = 'Invoke-Command localhost { Verb }' }
            @{ Boundary = 'Invoke-Command -Verbose localhost -ScriptBlock { Verb }' }
            @{ Boundary = 'Invoke-Command -Verbose:$true localhost -ScriptBlock { Verb }' }
            @{ Boundary = 'Invoke-Command -ConnectionUri http://server:5985/wsman -ScriptBlock { Verb }' }
            @{ Boundary = 'InlineScript { Verb }' }
            @{ Boundary = 'Script Test { GetScript = { Verb } }' }
        ) {
            param($Boundary)

            $target = "function Verb {}; $Boundary"
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "respects visible script-defined commands before callback scope rules" -skip:($PSVersionTable.PSVersion.Major -lt 7) {
            $target = 'function ForEach-Object { param([scriptblock]$Parallel) & $Parallel }; function Verb {}; ForEach-Object -Parallel { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target

            $target = 'function ForEach-Object { param([scriptblock]$Process) & $Process }; function private:Verb {}; 1 | ForEach-Object -Process { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
            Invoke-Formatter -ScriptDefinition $target -Settings $settings |
                Should -Be ($target.Replace('{ Verb }', '{ Get-Verb }'))
        }

        It "resolves dot-sourced callback commands before classifying nested callbacks" {
            $target = '. { function ForEach-Object { param([scriptblock]$Process) & $Process } }; ForEach-Object { function private:Verb {}; Verb }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings |
                Should -Be ($target.Substring(0, $target.LastIndexOf(' Verb')) + ' Get-Verb')
        }

        It "respects caller scope for script blocks inside array callback arguments" {
            $target = 'function private:Verb {}; 1 | ForEach-Object -Process {}, { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "respects caller scope for callbacks in array subexpressions" {
            $target = 'function private:Verb {}; 1 | ForEach-Object -Process @({ Verb })'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "does not index script blocks inside array input data" {
            $target = 'ForEach-Object -InputObject { function Verb {} }, { function Other {} } -Process { $_ }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "respects caller scope for trailing RemainingScripts callbacks" {
            $target = 'function private:Verb {}; 1 | ForEach-Object {}, {}, { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "resolves abbreviated and colon-attached parallel callback parameters" -skip:($PSVersionTable.PSVersion.Major -lt 7) {
            foreach ($parameter in @('-Pa { Verb }', '-Pa:{ Verb }')) {
                $target = "function Verb {}; 1 | ForEach-Object $parameter"
                $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

                $violations.Count | Should -Be 1
                $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
            }
        }

        It "recognizes functions declared in ForEach-Object parallel callbacks" -skip:($PSVersionTable.PSVersion.Major -lt 7) {
            $target = '1 | ForEach-Object -Parallel { function Verb {}; Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
        }

        It "does not index script blocks passed as ForEach-Object input data" {
            $target = 'ForEach-Object -InputObject { function Verb {} } -Process { $_ }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not index functions in constant-false branches" {
            $target = 'if ($false) { function Verb {} }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not classify Where-Object comparison values as executable callbacks" {
            $target = 'Where-Object -Property Name -EQ { function Verb {} }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not classify ForEach-Object MemberName arguments as executable callbacks" {
            $target = '1 | ForEach-Object -MemberName Equals { function Verb {} }; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not treat an explicitly disabled NoNewScope switch as caller-scoped" {
            $target = 'function private:Verb {}; Invoke-Command -NoNewScope:$false -ScriptBlock { Verb }'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "handles deeply nested caller-scoped callbacks without repeated visibility scans" {
            $depth = 12
            $target = ('1 | ForEach-Object { ' * $depth) + 'Verb' + (' }' * $depth)
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
        }

        It "indexes declarations in parameterized dot-sourced blocks in the caller scope" {
            $target = '. { param($value) function private:Verb {}; Verb } 1; Verb'
            $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName)

            $violations.Count | Should -Be 0
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }

        It "should return no violation for assignment statement-like command in dsc configuration" -skip:($IsLinux -or $IsMacOS) {
            $target = @'
Configuration MyDscConfiguration {
    Node "NodeA" {
        SomeResource MyResourceInstance {
            Type = "Present"
            Name =    "RSAT"
        }
    }
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName | `
                Get-Count | `
                Should -Be 0
        }

        It "does not expand DSC resource names during formatting" -skip:($IsLinux -or $IsMacOS) {
            $target = @'
Configuration PackageTest {
    Package PackageExample
    {
        Name = "TestPackage"
    }
}
'@
            $settings = @{ Rules = @{ PSAvoidUsingCmdletAliases = @{ Enable = $true } } }
            Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName |
                Where-Object { $_.RuleName -eq $violationName } |
                Get-Count | Should -Be 0
            Invoke-Formatter -ScriptDefinition $target -Settings $settings | Should -Be $target
        }
    }

    Context "DSC resource ASTs" {
        It "recognizes functions declared in the <Scope> scope as executable commands" -TestCases @(
            @{ Scope = 'script' }
            @{ Scope = 'configuration' }
        ) {
            param($Scope)

            $scriptFunction = if ($Scope -eq 'script') { 'function script:vErB {}' }
            $configurationFunction = if ($Scope -eq 'configuration') { 'function vErB {}' }
            $target = @"
$scriptFunction
{
    $configurationFunction
    Verb SameLine { ChildItem . {} }
    Verb FollowingLine
    {
        ChildItem . {}
    }
    Package PackageExample {}
}
"@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[-1].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            [System.Management.Automation.Language.StatementAst[]]$statements = if ($Scope -eq 'script') {
                @($ast.EndBlock.Statements[0].Copy(), $configuration)
            } else {
                @($configuration)
            }
            $scriptAst = [System.Management.Automation.Language.ScriptBlockAst]::new(
                $ast.Extent, $null,
                [System.Management.Automation.Language.StatementBlockAst]::new($ast.Extent, $statements, $null), $false)
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            $violations = @($rule.AnalyzeScript($scriptAst, $null))

            $violations.Count | Should -Be 2
            $violations | ForEach-Object {
                $_.Extent.Text | Should -Be 'ChildItem'
                $_.SuggestedCorrections[0].Text | Should -Be 'Get-ChildItem'
            }
        }

        It "does not treat functions declared in unrelated scopes as executable commands" -TestCases @(
            @{ FunctionName = 'Verb' }
            @{ FunctionName = 'script:Verb' }
            @{ FunctionName = 'global:Verb' }
        ) {
            param($FunctionName)

            $target = @"
{
    function Get-Example {
        function $FunctionName {}
    }
    Verb ResourceExample { ChildItem NestedResource {} }
}
"@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            @($rule.AnalyzeScript($configuration, $null)).Count | Should -Be 0
        }

        It "does not index functions in constant-false branches when identifying DSC resources" {
            $target = @'
{
    if ($false) { function Verb {} }
    Verb ResourceExample { ChildItem NestedResource {} }
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            @($rule.AnalyzeScript($configuration, $null)).Count | Should -Be 0
        }

        It "does not expand nested resource names in a same-line Node declaration when node is discoverable" -skip:(!(Get-Command node -CommandType Application -ErrorAction Ignore)) {
            $target = @'
{
    Node localhost { Package PackageExample {} }
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            @($rule.AnalyzeScript($configuration, $null)).Count | Should -Be 0
        }

        It "ignores resource declarations but not executable commands" {
            $target = @'
{
    Verb VerbExample {}
    Verb NextLineExample
    {
    }
    gci ResourceAliasExample
    {
        Name = "Example"
    }
    icm Server01 { Verb Argument {} }
    & Verb Argument {}
    function Get-Example {
        Verb Argument {}
    }
    Node localhost
    {
        Verb NestedExample {}
        Script ScriptExample {
            SetScript = {
                icm Server01 {}
                Verb Argument {}
            }
        }
    }
    $scriptProperties = @{
        SetScript = { Verb Argument {} }
    }
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            $violations = @($rule.AnalyzeScript($configuration, $null))

            $violations.Count | Should -Be 8
            $violations[0].Extent.Text | Should -Be 'gci'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-ChildItem'
            $violations[1].Extent.Text | Should -Be 'icm'
            $violations[2].Extent.Text | Should -Be 'Verb'
            $violations[3].Extent.Text | Should -Be 'Verb'
            $violations[4].Extent.Text | Should -Be 'Verb'
            $violations[5].Extent.Text | Should -Be 'icm'
            $violations[6].Extent.Text | Should -Be 'Verb'
            $violations[7].Extent.Text | Should -Be 'Verb'
        }

        It "does not ignore executable commands in assignment and expression contexts or pipelines" {
            $target = @'
{
    $items = ChildItem . {}
    $result = Verb Argument {}
    $action = { Verb Argument {} }
    Name = (Verb Argument {})
    Script ScriptExample {
        SetScript =
        {
            Verb Argument {}
        }
    }
    Verb Argument {} | Out-Null
    gci . | Out-Null
    {
    }
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            $violations = @($rule.AnalyzeScript($configuration, $null))

            $violations.Count | Should -Be 7
            $violations[0].Extent.Text | Should -Be 'ChildItem'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-ChildItem'
            $violations[1].Extent.Text | Should -Be 'Verb'
            $violations[2].Extent.Text | Should -Be 'Verb'
            $violations[3].Extent.Text | Should -Be 'Verb'
            $violations[4].Extent.Text | Should -Be 'Verb'
            $violations[5].Extent.Text | Should -Be 'Verb'
            $violations[6].Extent.Text | Should -Be 'gci'
        }

        It "does not mistake executable pipelines for DSC resources in fallback configuration ASTs" {
            $target = @'
{
    Write-Output $(Verb Argument {})
    @(Verb Argument {})
    Verb Argument {} > $null
    Verb Argument {} &
    if (Verb Argument {}) {}
    return Verb Argument {}
    if ($true) {
        Verb SameLineResource {}
        Verb NextLineResource
        {
        }
    }
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            $violations = @($rule.AnalyzeScript($configuration, $null))

            $expectedViolations = if ($PSVersionTable.PSVersion.Major -ge 7) { 6 } else { 5 }
            $violations.Count | Should -Be $expectedViolations
            $violations | ForEach-Object {
                $_.SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
            }
        }

        It "does not mistake following-line blocks after executable commands for DSC resources" {
            $target = @'
{
    Write-Output value
    {
        Verb Argument {}
    }
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            $violations = @($rule.AnalyzeScript($configuration, $null))

            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'Verb'
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Get-Verb'
        }

        It "does not pair following-line blocks with commands when the block has redirections or a statement separator intervenes" {
            $target = @'
{
    gci .
    {} > $null
    gci .; {}
    gci . &
    {}
    gci .
    {} &
}
'@
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($target, [ref]$null, [ref]$null)
            $body = $ast.EndBlock.Statements[0].PipelineElements[0].Expression.Copy()
            $configuration = [System.Management.Automation.Language.ConfigurationDefinitionAst]::new(
                $ast.Extent, $body, 'Resource',
                [System.Management.Automation.Language.StringConstantExpressionAst]::new($ast.Extent, 'Test', 'BareWord'))
            $rule = [Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.AvoidAlias]::new()

            $violations = @($rule.AnalyzeScript($configuration, $null))

            $violations.Count | Should -Be 4
            $violations | ForEach-Object {
                $_.Extent.Text | Should -Be 'gci'
                $_.SuggestedCorrections[0].Text | Should -Be 'Get-ChildItem'
            }
        }
    }

    Context "Settings file provides allowlist" {
        BeforeAll {
            $allowListTestScriptDef = 'gci; cd;'
            $settings = @{
                'Rules' = @{
                    'PSAvoidUsingCmdletAliases' = @{
                        'allowlist' = @('cd')
                    }
                }
            }
        }

        It "honors the allowlist provided as hashtable" {
            $settings = @{
                'Rules' = @{
                    'PSAvoidUsingCmdletAliases' = @{
                        'allowlist' = @('cd')
                    }
                }
            }
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $allowListTestScriptDef -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
        }

        It "honors the allowlist provided through settings file" {
            # even though join-path returns string, if we do not use tostring, then invoke-scriptanalyzer cannot cast it to string type
            $settingsFilePath = (Join-Path $PSScriptRoot (Join-Path 'TestSettings' 'AvoidAliasSettings.psd1')).ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $allowListTestScriptDef -Settings $settingsFilePath -IncludeRule $violationName
            $violations.Count | Should -Be 1
        }

        It "honors the allowlist in a case-insensitive manner" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition "CD" -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "do not warn when about Get-* completed cmdlets when the command exists natively on Unix platforms" -skip:(-not ($IsLinux -or $IsMacOS)) {
            $nativeLinuxCommand = 'date'
            $dateCommand = Get-Command $nativeLinuxCommand -CommandType Application -ErrorAction Ignore

            $violations = Invoke-ScriptAnalyzer -IncludeRule PSAvoidUsingCmdletAliases -ExcludeRule PSUseCompatibleCommands -ScriptDefinition $nativeLinuxCommand | Where-Object { $_.RuleName -eq $violationName }

            $expectedViolations = 1
            if ($dateCommand)
            {
                $expectedViolations = 0
            }

            $violations.Count | Should -Be $expectedViolations
        }

        It 'Warn about incorrect syntax around process block' {
            $scriptDefinition = { function foo { IShouldNotBeHere; process {} } }
            $violations = Invoke-ScriptAnalyzer -IncludeRule PSAvoidUsingCmdletAliases -ScriptDefinition "$scriptDefinition"
            $violations.Count | Should -Be 1
            $violations.Severity | Should -Be ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity]::ParseError)
        }
    }
}
