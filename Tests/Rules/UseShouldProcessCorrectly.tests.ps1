# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

$IsV3OrV4 = ($PSVersionTable.PSVersion.Major -eq 3) -or ($PSVersionTable.PSVersion.Major -eq 4)

BeforeAll {
    $violationMessage = "'Verb-Files' has the ShouldProcess attribute but does not call ShouldProcess/ShouldContinue."
    $violationName = "PSShouldProcess"

    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "UseShouldProcessCorrectly" {
    Context "When there are violations" {
        It "has 3 should process violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }

    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }

    Context "Where ShouldProcess is called by a downstream function" {
        It "finds no violation for 1 level downstream call" {
            $scriptDef = @'
function Foo
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param()

    Bar
}

function Bar
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param()

    if ($PSCmdlet.ShouldProcess(""))
    {
        "Continue normally..."
    }
    else
    {
        "what would happen..."
    }
}

Foo
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }

        It "finds violation if downstream function does not declare SupportsShouldProcess" {
              $scriptDef = @'
function Foo
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param()

    Bar
}

function Bar
{
    if ($PSCmdlet.ShouldProcess(""))
    {
        "Continue normally..."
    }
    else
    {
        "what would happen..."
    }
}

Foo
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 1
        }

        It "finds violation for 2 level downstream calls" {
            $scriptDef = @'
function Foo
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param()

    Baz
}

function Baz
{
    Bar
}

function Bar
{
    if ($PSCmdlet.ShouldProcess(""))
    {
        "Continue normally..."
    }
    else
    {
        "what would happen..."
    }
}

Foo
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 1
        }
    }

    Context "When nested function definition calls ShouldProcess" {
        It "finds no violation" {
            $scriptDef = @'
function Foo
{
   [CmdletBinding(SupportsShouldProcess)]
   param()
   begin
   {
       function Bar
       {
           if ($PSCmdlet.ShouldProcess('',''))
           {

           }
       }
       bar
   }
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }
    }

    Context "When a builtin command that supports ShouldProcess is called" {
        It "finds no violation when caller declares SupportsShouldProcess and callee is a cmdlet with ShouldProcess" {
            $scriptDef = @'
function Remove-Foo {
[CmdletBinding(SupportsShouldProcess)]
    Param(
        [string] $Path
    )
    Write-Verbose "Removing $($path)"
    Remove-Item -Path $Path
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }

        It "finds no violation when caller does not declare SupportsShouldProcess and callee is a cmdlet with ShouldProcess" {
            $scriptDef = @'
function Remove-Foo {
    Param(
        [string] $Path
    )
    Write-Verbose "Removing $($path)"
    Remove-Item -Path $Path
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }

        # Install-Module is present by default only on PSv5 and above
        It "finds no violation when caller declares SupportsShouldProcess and callee is a function with ShouldProcess" -Skip:$IsV3OrV4 {
            $scriptDef = @'
function Install-Foo {
[CmdletBinding(SupportsShouldProcess)]
    Param(
        [string] $ModuleName
    )
    Install-Module $ModuleName
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }

        It "finds no violation when caller does not declare SupportsShouldProcess and callee is a function with ShouldProcess" {
            $scriptDef = @'
function Install-Foo {
    Param(
        [string] $ModuleName
    )
    Install-Module $ModuleName
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }

       It "finds no violation for a function with self reference" {
            $scriptDef = @'
function Install-ModuleWithDeps {
    [CmdletBinding(SupportsShouldProcess)]
    Param(
        [Parameter(ValueFromPipeline)]
        [string] $ModuleName
    )
    if ($PSCmdlet.ShouldProcess("Install module with dependencies"))
    {
        Get-Dependencies $ModuleName | Install-ModuleWithDeps
        Install-ModuleCustom $ModuleName
    }
    else
    {
        Get-Dependencies $ModuleName | Install-ModuleWithDeps
        Write-Host ("Would install module {0}" -f $ModuleName)
    }
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }

       # Install-Module is present by default only on PSv5 and above
       It "finds no violation for a function with self reference and implicit call to ShouldProcess" -Skip:$IsV3OrV4 {
            $scriptDef = @'
function Install-ModuleWithDeps {
[CmdletBinding(SupportsShouldProcess)]
    Param(
        [Parameter(ValueFromPipeline)]
        [string] $ModuleName
    )
    $deps = Get-Dependencies $ModuleName
    if ($deps -eq $null)
    {
        Install-Module $ModuleName
    }
    else
    {
        $deps | Install-ModuleWithDeps
    }
    Install-Module $ModuleName
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations.Count | Should -Be 0
        }
    }

    Context "Violation Extent" {
        It "should mark only the SupportsShouldProcess attribute" {
            $scriptDef = @'
function Foo
{
   [CmdletBinding(SupportsShouldProcess)]
   param()
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations[0].Extent.Text | Should -Be 'SupportsShouldProcess'
        }

        It "should mark only the ShouldProcess call" {
             $scriptDef = @'
function Foo
{
   param()
   if ($PSCmdlet.ShouldProcess('','')) { Write-Output "Should Process" }
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule PSShouldProcess
            $violations[0].Extent.Text | Should -Be 'ShouldProcess'
        }
    }
}
