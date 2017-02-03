# UseWhitespace
**Severity Level: Warning**

## Description
This rule enforces consistent brace, parenthesis, binary operator, assignment operator and separator styles. For more information please see the [Parameters](#parameters) section.

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration
```powershell
    Rules = @{
        PSUseWhitespace = @{
            Enable = $true
            CheckOpenBrace = $true
            CheckOpenParen = $true
            CheckOperator = $true
            CheckSeparator = $true
        }
```

### Parameters

#### Enable: bool (Default value is `$false`)
Enable or disable the rule during ScriptAnalyzer invocation.

#### CheckOpenBrace: bool (Default value is `$true`)
Checks if there is a space between a keyword and its corresponding open brace. E.g. `foo { }`.

#### CheckOpenParen: bool (Default value is `$true`)
Checks if there is space between a keyword and its corresponding open parenthesis. E.g. `if (true)`.

#### CheckOperator: bool (Default value is `$true`)
Checks if a binary operator is surrounded on both sides by a space. E.g. `$x = 1`.

#### CheckSeparator: bool (Default value is `$true`)
Checks if a comma or a semicolon is followed by a space. E.g. `@(1, 2, 3)` or `@{a = 1; b = 2}`.