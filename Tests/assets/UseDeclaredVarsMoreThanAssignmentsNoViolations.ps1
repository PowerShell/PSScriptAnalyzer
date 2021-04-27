$declaredVars = "Declared Vars"
Write-Ouput $declaredVars
$script:thisshouldnotraiseerrors = "this should not raise errors"
$foo.property = "This also should not raise errors"

# Output field separator builtin special variable
$OFS = ', '
[string]('apple', 'banana', 'orange')

# Using scope
$private:x = 42
$x

$variable:a = 52
$a

#function
$function:prompt = [ScriptBlock]::Create($newPrompt)