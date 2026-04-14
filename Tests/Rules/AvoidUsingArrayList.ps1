using namespace system.collections

# Using New-Object
$List = New-Object ArrayList
$List = New-Object 'ArrayList'
$List = New-Object "ArrayList"
$List = New-Object -Type ArrayList
$List = New-Object -TypeName ArrayLIST
$List = New-Object Collections.ArrayList
$List = New-Object System.Collections.ArrayList

# Using type initializer
$List = [ArrayList](1,2,3)
$List = [ArrayLIST]@(1,2,3)
$List = [ArrayList]::new()
$List = [Collections.ArrayList]::New()
$List = [System.Collections.ArrayList]::new()
1..3 | ForEach-Object { $null = $List.Add($_) }
