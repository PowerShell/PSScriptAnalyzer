# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'Suppressions on interleaved declarations' {
    It 'suppresses function and class bodies without suppressing a call outside them' {
        $definition = @'
function First {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingInvokeExpression', '')]
    param()
    Invoke-Expression 'first'
}
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingInvokeExpression', '')]
class SuppressedClass {
    [void] Emit() {
        Invoke-Expression 'class'
    }
}
function Last {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingInvokeExpression', '')]
    param()
    Invoke-Expression 'last'
}
Invoke-Expression 'outside'
'@
        $violations = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -IncludeRule PSAvoidUsingInvokeExpression)
        $violations.Count | Should -Be 1
        $violations[0].Extent.Text | Should -BeExactly "Invoke-Expression 'outside'"

        $suppressed = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -IncludeRule PSAvoidUsingInvokeExpression -SuppressedOnly)
        $suppressed.Count | Should -Be 3
    }
}
