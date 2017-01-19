# PlaceCloseBrace
**Severity Level: Warning**

## Description
Close brace placement should follow a consistent style. It should be on a new line by itself and should not be followed by an empty line.

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration
```powershell
    Rules = @{
        PSPlaceCloseBrace = @{
            Enable = $true
            NoEmptyLineBefore = $false
        }
```

### Parameters

#### Enable: bool (Default value is `$false`)
Enable or disable the rule during ScriptAnalyzer invocation.

#### NoEmptyLineBefore: bool (Default value is `$false`)
Create violation if there is an empty line before a close brace.
