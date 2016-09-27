Import-Module PSScriptAnalyzer

Describe "Test Directed Graph" {
    Context "When a graph is created" {
        $digraph = New-Object -TypeName 'Microsoft.Windows.PowerShell.ScriptAnalyzer.DiGraph'
        0..4 | ForEach-Object {$digraph.AddVertex()}

        $digraph.AddEdge(0, 1);
        $digraph.AddEdge(0, 4);
        $digraph.AddEdge(1, 3);

        It "correctly adds the vertices" {
            $digraph.GetNumVertices() | Should Be 5
        }

        It "correctly adds the edges" {
            $digraph.GetNumNeighbors(0) | Should Be 2
            $neighbors = $digraph.GetNeighbors(0)
            $neighbors -contains 1 | Should Be $true
            $neighbors -contains 4 | Should Be $true
        }

        It "finds the connection" {
            $digraph.IsConnected(0, 3) | Should Be $true
        }
    }
}