Get-ChildItem  -path $path
Get-ChildItem     -path $path     | Should -Be $exp
Get-ChildItem     -path $path     | Should -Be $exp
foo|bar      # addWhitespaceAroundPipe
foo   |   bar  # trimWhitespaceAroundPipe
foo   -barc  # whitespaceBetweenParameters
if ($true) {}

foo |
        bar

foo |
                bar

foo |
bar

foo |
bar |
baz
