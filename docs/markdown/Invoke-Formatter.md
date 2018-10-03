---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
schema: 2.0.0
---

# Invoke-Formatter
## SYNOPSIS
Formats a script text based on the input settings or default settings.

## SYNTAX

```
Invoke-Formatter [-ScriptDefinition] <String> [-Settings <object>] [-Range <int[]>]
```

## DESCRIPTION

The Invoke-Formatter cmdlet takes a string parameter named ScriptDefinition and formats it according to the input settings parameter Settings. If no Settings parameter is provided, the cmdlet assumes the default code formatting settings as defined in Settings/CodeFormatting.psd1.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
$scriptDefinition = @'
function foo {
"hello"
  }
'@

Invoke-Formatter -ScriptDefinition $scriptDefinition
```

This command formats the input script text using the default settings.

### -------------------------- EXAMPLE 2 --------------------------
```
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

This command formats the input script text using the settings defined in the $settings hashtable.

### -------------------------- EXAMPLE 3 --------------------------
```
Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings /path/to/settings.psd1
```

This command formats the input script text using the settings defined in the settings.psd1 file.

## PARAMETERS

### -ScriptDefinition
The script text to be formated.

*NOTE*: Unlike ScriptBlock parameter, the ScriptDefinition parameter require a string value.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

### -Settings
A settings hashtable or a path to a PowerShell data file (.psd1) file that contains the settings.

```yaml
Type: Object
Parameter Sets: (All)

Required: False
Position: 2
Default value: CodeFormatting
Accept pipeline input: False
Accept wildcard characters: False
```

### -Range
The range within which formatting should take place. The parameter is an array of integers of length 4 such that the first, second, third and last elements correspond to the start line number, start column number, end line number and end column number. These numbers must be greater than 0.

```yaml
Type: Int32[]
Parameter Sets: (All)

Required: False
Position: 3
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

## OUTPUTS

### System.String
The formatted string result.
