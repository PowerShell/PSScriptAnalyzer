---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
Module Name: PSScriptAnalyzer
ms.date: 10/07/2021
online version: https://docs.microsoft.com/powershell/module/psscriptanalyzer/invoke-formatter?view=ps-modules&wt.mc_id=ps-gethelp
schema: 2.0.0
---

# Invoke-Formatter

## SYNOPSIS
Formats a script text based on the input settings or default settings.

## SYNTAX

```
Invoke-Formatter [-ScriptDefinition] <string> [[-Settings] <Object>] [[-Range] <int[]>]
 [<CommonParameters>]
```

## DESCRIPTION

The `Invoke-Formatter` cmdlet takes a string input and formats it according to defined settings. If
no **Settings** parameter is provided, the cmdlet assumes the default code formatting settings as
defined in `Settings/CodeFormatting.psd1`.

## EXAMPLES

### EXAMPLE 1 - Format the input script text using the default settings

```powershell
$scriptDefinition = @'
function foo {
"hello"
  }
'@

Invoke-Formatter -ScriptDefinition $scriptDefinition
```

```Output
function foo {
    "hello"
}
```

### EXAMPLE 2 - Format the input script using the settings defined in a hashtable

```powershell
$scriptDefinition = @'
function foo {
"hello"
}
'@

$settings = @{
    IncludeRules = @("PSPlaceOpenBrace", "PSUseConsistentIndentation")
    Rules = @{
        PSPlaceOpenBrace = @{
            Enable = $true
            OnSameLine = $false
        }
        PSUseConsistentIndentation = @{
            Enable = $true
        }
    }
}

Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings $settings
```

```Output
function foo
{
    "hello"
}
```

### EXAMPLE 3 - Format the input script text using the settings defined a `.psd1` file

```powershell
Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings /path/to/settings.psd1
```

## PARAMETERS

### -Range

The range within which formatting should take place. The value of this parameter must be an array of
four integers. These numbers must be greater than 0. The four integers represent the following four
values in this order:

- starting line number
- starting column number
- ending line number
- ending column number

```yaml
Type: Int32[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ScriptDefinition

The text of the script to be formatted represented as a string. This is not a **ScriptBlock**
object.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Settings

A settings hashtable or a path to a PowerShell data file (`.psd1`) that contains the settings.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: CodeFormatting
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### System.String

The formatted string result.

## NOTES

## RELATED LINKS
