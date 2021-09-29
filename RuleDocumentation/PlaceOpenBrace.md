# PlaceOpenBrace

**Severity Level: Warning**

## Description

Open brace placement should follow a consistent style. It can either follow K&R style (on same line)
or the Allman style (not on same line).

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration

```powershell
Rules = @{
    PSPlaceOpenBrace = @{
        Enable = $true
        OnSameLine = $true
        NewLineAfter = $true
        IgnoreOneLineBlock = $true
    }
}
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### OnSameLine: bool (Default value is `$true`)

Enforce open brace to be on the same line as that of its preceding keyword.

#### NewLineAfter: bool (Default value is `$true`)

Enforce a new line character after an open brace. The default value is true.

#### IgnoreOneLineBlock: bool (Default value is `$true`)

Indicates if open braces in a one line block should be ignored or not. For example,
` $x = if ($true) { "blah" } else { "blah blah" }`, if the property is set to true then the rule
doesn't fire a violation.
