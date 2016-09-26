Import-Module PSScriptAnalyzer
$helperNamespace = 'Microsoft.Windows.PowerShell.ScriptAnalyzer';


Describe "Test Directed Graph" {
    Context "When a graph is created" {
        $digraph = New-Object -TypeName 'Microsoft.Windows.PowerShell.ScriptAnalyzer.DiGraph[string]'
        $digraph.AddVertex('v1');
        $digraph.AddVertex('v2');
        $digraph.AddVertex('v3');
        $digraph.AddVertex('v4');
        $digraph.AddEdge('v1', 'v2');
        $digraph.AddEdge('v2', 'v4');

        It "correctly adds the vertices" {
            $digraph.NumVertices | Should Be 4
        }

        It "correctly adds the edges" {
            $digraph.GetNumNeighbors('v1') | Should Be 1
            $neighbors = $digraph.GetNeighbors('v1')
            $neighbors[0] | Should Be 'v2'
        }

        It "finds the connection" {
            $digraph.IsConnected('v1', 'v4') | Should Be $true
        }
    }
}