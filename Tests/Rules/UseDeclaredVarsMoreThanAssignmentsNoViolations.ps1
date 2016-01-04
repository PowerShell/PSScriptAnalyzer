$declaredVars = "Declared Vars"
Write-Ouput $declaredVars
$script:thisshouldnotraiseerrors = "this should not raise errors"
$foo.property = "This also should not raise errors"

# Output field separator builtin special variable
$OFS = ', '
[string]('apple', 'banana', 'orange')