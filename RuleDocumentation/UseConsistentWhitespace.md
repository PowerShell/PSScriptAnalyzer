# UseConsistentWhitespace

**Severity Level: Warning**

## Description

# Parameters

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration

```powershell
    Rules = @{
        PSUseConsistentWhitespace  = @{
            Enable          = $true
            CheckInnerBrace = $true
            CheckOpenBrace  = $true
            CheckOpenParen  = $true
            CheckOperator   = $true
            CheckPipe       = $true
            CheckSeparator  = $true
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

Checks if a pipe is surrounded on both sides by a space. E.g. `foo | bar` instead of `foo|bar`.
