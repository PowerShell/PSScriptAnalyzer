# PlaceOpenBrace
**Severity Level: Warning**

## Description
Open brace placement should follow a consistent style. It can either follow KR style (on same line) or the Allman style (not on same line).

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration
```powershell
    Rules = @{
        PSPlaceOpenBrace = @{
            Enable = $true
            OnSameLine = $true
            NewLineAfter = $true
        }
```

### Parameters

#### Enable: bool (Default value is `$false`)
Enable or disable the rule during ScriptAnalyzer invocation.

#### OnSameLine: bool (Default value is `$true`)
Provide an option to enforce the open brace to be on the same line as preceding keyword (KR style) or on the next line (Allman style). The default value is `#true.

#### NewLineAfter: bool (Default value is `$true`)
Enforce a new line character after an open brace. The default value is true.
