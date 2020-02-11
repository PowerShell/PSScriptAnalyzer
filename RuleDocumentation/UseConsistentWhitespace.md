# UseConsistentWhitespace

**Severity Level: Warning**

## Description

# Parameters

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration

```powershell
    Rules = @{
        PSUseConsistentWhitespace  = @{
            Enable                          = $true
            CheckInnerBrace                 = $true
            CheckOpenBrace                  = $true
            CheckOpenParen                  = $true
            CheckOperator                   = $true
            CheckPipe                       = $true
            CheckPipeForRedundantWhitespace = $false
            CheckSeparator                  = $true
            CheckParameter                  = $false
        }
    }
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### CheckInnerBrace: bool (Default value is `$true`)

Checks if there is a space after the opening brace and a space before the closing brace. E.g. `if ($true) { foo }` instead of `if ($true) {bar}`.

#### CheckOpenBrace: bool (Default value is `$true`)

Checks if there is a space between a keyword and its corresponding open brace. E.g. `foo { }` instead of `foo{ }`.

#### CheckOpenParen: bool (Default value is `$true`)

Checks if there is space between a keyword and its corresponding open parenthesis. E.g. `if (true)` instead of `if(true)`.

#### CheckOperator: bool (Default value is `$true`)

Checks if a binary or unary operator is surrounded on both sides by a space. E.g. `$x = 1` instead of `$x=1`.

#### CheckSeparator: bool (Default value is `$true`)

Checks if a comma or a semicolon is followed by a space. E.g. `@(1, 2, 3)` or `@{a = 1; b = 2}` instead of `@(1,2,3)` or `@{a = 1;b = 2}`.

#### CheckPipe: bool (Default value is `$true`)

Checks if a pipe is surrounded on both sides by a space but ignores redundant whitespace. E.g. `foo | bar` instead of `foo|bar`.

#### CheckPipeForRedundantWhitespace : bool (Default value is `$false`)

Checks if a pipe is surrounded by redundant whitespace (i.e. more than 1 whitespace). E.g. `foo | bar` instead of `foo    |    bar`.

#### CheckParameter: bool (Default value is `$false` at the moment due to the setting being new)

Checks if there is more than one space between parameters and values. E.g. `foo -bar $baz -bat` instead of `foo   -bar   $baz   -bat`. This eliminates redundant whitespace that was probably added unintentionally.
The rule does not check for whitespace between parameter and value when the colon syntax `-ParameterName:$ParameterValue` is used as some users prefer either 0 or 1 whitespace in this case.
