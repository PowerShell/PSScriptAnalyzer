Import-Module PSScriptAnalyzer


Function ConvertType($x)
{
    $z = [System.Collections.Generic.List[Tuple[int,int]]]::new()
    $x | ForEach-Object {$z.Add([System.Tuple[int,int]]::new($_[0], $_[1]))}
    return $z
}

Describe "Test Directed Graph" {
    Context "When a graph is created" {
        $edges = ConvertType (0,1),(0,4),(1,3)
        $digraph = New-Object -TypeName 'Microsoft.Windows.PowerShell.ScriptAnalyzer.DiGraph' -ArgumentList 5,$edges
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