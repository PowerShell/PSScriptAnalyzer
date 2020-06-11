# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Test Directed Graph" {
    Context "When a graph is created" {
        BeforeAll {
            $digraph = New-Object -TypeName 'Microsoft.Windows.PowerShell.ScriptAnalyzer.DiGraph[string]'
            $digraph.AddVertex('v1');
            $digraph.AddVertex('v2');
            $digraph.AddVertex('v3');
            $digraph.AddVertex('v4');
            $digraph.AddVertex('v5');

            $digraph.AddEdge('v1', 'v2');
            $digraph.AddEdge('v1', 'v5');
            $digraph.AddEdge('v2', 'v4');
        }

        It "correctly adds the vertices" {
            $digraph.NumVertices | Should -Be 5
        }

        It "correctly adds the edges" {
            $digraph.GetOutDegree('v1') | Should -Be 2
            $neighbors = $digraph.GetNeighbors('v1')
            $neighbors -contains 'v2' | Should -BeTrue
            $neighbors -contains 'v5' | Should -BeTrue
        }

        It "finds the connection" {
            $digraph.IsConnected('v1', 'v4') | Should -BeTrue
        }
    }

    Context "Runspaces should be disposed" {
        It "Running analyzer 100 times should only create a limited number of runspaces" -Skip:$($PSVersionTable.PSVersion.Major -le 4) {
            $null = 1..100 | ForEach-Object { Invoke-ScriptAnalyzer -ScriptDefinition 'gci' }
            (Get-Runspace).Count | Should -BeLessOrEqual 14 -Because 'Number of Runspaces should be bound (size of runspace pool cache is 10)'
        }
    }
}
