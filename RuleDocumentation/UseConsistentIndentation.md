# UseConsistentIndentation

**Severity Level: Warning**

## Description

Indentation should be consistent throughout the source file.

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## Configuration

```powershell
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
            Kind = 'space'
        }
    }
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### IndentationSize: int (Default value is `4`)

Indentation size in the number of space characters.

#### Kind: string (Default value is `space`)

Represents the kind of indentation to be used. Possible values are: `space`, `tab`. If any invalid value is given, the property defaults to `space`.

`space` means `IndentationSize` number of `space` characters are used to provide one level of indentation.
`tab` means a tab character, `\t`.
