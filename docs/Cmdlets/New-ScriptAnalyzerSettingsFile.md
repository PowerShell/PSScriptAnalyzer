---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
Module Name: PSScriptAnalyzer
ms.date: 04/17/2026
schema: 2.0.0
---

# New-ScriptAnalyzerSettingsFile

## SYNOPSIS
Creates a new PSScriptAnalyzer settings file.

## SYNTAX

```
New-ScriptAnalyzerSettingsFile [[-Path] <string>] [-BaseOnPreset <string>] [-Force] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

The `New-ScriptAnalyzerSettingsFile` cmdlet creates a `PSScriptAnalyzerSettings.psd1` file in the
specified directory.

When the **BaseOnPreset** parameter is provided, the generated file contains the rules and
configuration defined by the given preset.

When **BaseOnPreset** is not provided, the generated file includes all current rules in the
`IncludeRules` list and populates the `Rules` section with all configurable properties, set to their
default values. Both modes also include `CustomRulePath`, `RecurseCustomRulePath`, and
`IncludeDefaultRules` keys with descriptive comments so the file is immediately ready for
customisation.

If a settings file already exists at the target path, the cmdlet emits a terminating error unless
the **Force** parameter is specified - in which case it is overwritten.

## EXAMPLES

### EXAMPLE 1 - Create a default settings file in the current directory

```powershell
New-ScriptAnalyzerSettingsFile
```

Creates `PSScriptAnalyzerSettings.psd1` in the current working directory incluindg all rules and
all configurable options set to their defaults.

### EXAMPLE 2 - Create a settings file based on a preset

```powershell
New-ScriptAnalyzerSettingsFile -BaseOnPreset CodeFormatting
```

Creates a settings file pre-populated with the rules and configuration from the `CodeFormatting`
preset.

### EXAMPLE 3 - Create a settings file in a specific directory

```powershell
New-ScriptAnalyzerSettingsFile -Path ./src/MyModule
```

Creates the settings file in the `./src/MyModule` directory.

### EXAMPLE 4 - Preview the operation without creating the file

```powershell
New-ScriptAnalyzerSettingsFile -WhatIf
```

Shows what the cmdlet would do without actually writing the file.

## PARAMETERS

### -Path

The directory where the settings file will be created. Defaults to the current working directory when not specified.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: Current directory
Accept pipeline input: False
Accept wildcard characters: False
```

### -BaseOnPreset

The name of a built-in preset to use as the basis for the generated settings file. Valid values are
discovered at runtime from the shipped preset files and can be tab-completed in the shell.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf

Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force

Overwrite an existing settings file at the target path.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm

Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.IO.FileInfo

The cmdlet returns a **FileInfo** object representing the created settings file.

## NOTES

The output file is always named `PSScriptAnalyzerSettings.psd1` so that the automatic settings
discovery in `Invoke-ScriptAnalyzer` picks it up when analysing scripts in the same directory.

Note: Relative paths in `CustomRulePath` are resolved from the caller's current working directory,
not from the location of the settings file. This matches `Invoke-ScriptAnalyzer` behaviour.

## RELATED LINKS

[Invoke-ScriptAnalyzer](Invoke-ScriptAnalyzer.md)

[Get-ScriptAnalyzerRule](Get-ScriptAnalyzerRule.md)

[Invoke-Formatter](Invoke-Formatter.md)

[Test-ScriptAnalyzerSettingsFile](Test-ScriptAnalyzerSettingsFile.md)
