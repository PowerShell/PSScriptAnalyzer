using namespace System.Collections.Generic

# Using a generic List
$List = New-Object List[Object]
1..3 | ForEach-Object { $List.Add($_) } # This will not return anything

$List = [List[Object]]::new()
1..3 | ForEach-Object { $List.Add($_) } # This will not return anything

# Creating a fixed array by using the PowerShell pipeline
$List = 1..3 | ForEach-Object { $_ }

# This should not violate because there isn't a
# `using namespace System.Collections` directive
# and ArrayList could belong to another namespace
$List = New-Object ArrayList
$List = [ArrayList](1,2,3)
$List = [ArrayList]@(1,2,3)
$List = [ArrayList]::new()