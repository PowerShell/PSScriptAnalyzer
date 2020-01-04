# AvoidLongLines

**Severity Level: Warning**

## Description

Lines should be no longer than a configured number of characters (default: 120), including leading whitespace (indentation).

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration

```powershell
    Rules = @{
        PSAvoidLongLines  = @{
            Enable     = $true
            LineLength = 120
        }
    }
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### MaximumLineLength: int (Default value is 120)

Optional parameter to override the default maximum line length.
