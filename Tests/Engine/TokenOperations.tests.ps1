# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "TokenOperations" {
    It "Should return correct AST position for assignment operator in hash table" {
        $scriptText = @'
$h = @{
    a = 72
    b = @{ z = "hi" }
}
'@
        $tokens = $null
        $parseErrors = $null
        $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($scriptText, [ref] $tokens, [ref] $parseErrors)
        $tokenOperations = New-Object Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations -ArgumentList @($tokens, $scriptAst)
        $operatorToken = $tokens | Where-Object { $_.Extent.StartLineNumber -eq 3 -and $_.Extent.StartColumnNumber -eq 14}
        $hashTableAst = $tokenOperations.GetAstPosition($operatorToken)
        $hashTableAst | Should -BeOfType [System.Management.Automation.Language.HashTableAst]
        $hashTableAst.Extent.Text | Should -Be '@{ z = "hi" }'
    }
}