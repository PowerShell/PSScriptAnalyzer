# PlaceOpenBrace
**Severity Level: Warning**

## Description
Indentation should be consistent throughout the source file.

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration
```powershell
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = $true
            NoEmptyLineBefore = $false
        }
```

### Parameters

#### Enable: bool (Default value is `$false`)
Enable or disable the rule during ScriptAnalyzer invocation.

#### IndentationSize: bool (Default value is `4`)
Indentation size in the number of space characters.
