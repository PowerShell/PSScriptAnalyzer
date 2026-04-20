---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
Module Name: PSScriptAnalyzer
ms.date: 04/17/2026
schema: 2.0.0
---

# Test-ScriptAnalyzerSettingsFile

## SYNOPSIS
Validates a PSScriptAnalyzer settings file.

## SYNTAX

```
Test-ScriptAnalyzerSettingsFile [-Path] <string> [-Quiet] [-CustomRulePath <string[]>]
 [-RecurseCustomRulePath] [<CommonParameters>]
```

## DESCRIPTION

The `Test-ScriptAnalyzerSettingsFile` cmdlet checks whether a PSScriptAnalyzer settings file is
valid. It verifies that:

- The file can be parsed as a PowerShell data file.
- All rule names referenced in `IncludeRules`, `ExcludeRules`, and `Rules` correspond to known
  rules (wildcard patterns are skipped).
- All `Severity` values are valid.
- Rule option names in the `Rules` section correspond to actual configurable properties.
- Rule option values that are constrained to a set of choices contain a valid value.

By default the cmdlet returns `$true` when the file is valid and writes non-terminating errors
describing each problem found when the file is invalid (no output is returned on failure, only
errors). This allows `$ErrorActionPreference = 'Stop'` to turn validation failures into
terminating errors.

When `-Quiet` is specified the cmdlet returns only `$true` or `$false` and suppresses all
error output.

## EXAMPLES

### EXAMPLE 1 - Validate a settings file

```powershell
Test-ScriptAnalyzerSettingsFile -Path ./PSScriptAnalyzerSettings.psd1
```

Returns `$true` if the file is valid. Writes non-terminating errors describing any problems found.

### EXAMPLE 2 - Validate quietly in a conditional

```powershell
if (Test-ScriptAnalyzerSettingsFile -Path ./PSScriptAnalyzerSettings.psd1 -Quiet) {
    Invoke-ScriptAnalyzer -Path ./src -Settings ./PSScriptAnalyzerSettings.psd1
}
```

Returns `$true` or `$false` without writing any errors.

### EXAMPLE 3 - Validate with custom rules

```powershell
Test-ScriptAnalyzerSettingsFile -Path ./Settings.psd1 -CustomRulePath ./MyRules
```

Validates the settings file whilst also considering rules from the `./MyRules` path.

## PARAMETERS

### -Path

The path to the settings file to validate.

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

### -Quiet

Suppresses error output and returns only `$true` or `$false`. Without this switch the cmdlet
writes non-terminating errors for each problem found and returns `$true` only when the file is
valid.

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

### -CustomRulePath

Paths to modules or directories containing custom rules. When specified, custom rule names are
treated as valid during validation.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: CustomizedRulePath

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecurseCustomRulePath

Search sub-folders under the custom rule path for additional rules.

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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Boolean

Returns `$true` when the settings file is valid. Without `-Quiet`, no output is returned when the
file is invalid - problems are reported as non-terminating errors. With `-Quiet`, always returns
`$true` or `$false`.

## NOTES

Without `-Quiet`, validation problems are reported as non-terminating errors. This means they
respect `$ErrorActionPreference` and can be promoted to terminating errors by setting
`-ErrorAction Stop`.

## RELATED LINKS

[New-ScriptAnalyzerSettingsFile](New-ScriptAnalyzerSettingsFile.md)

[Invoke-ScriptAnalyzer](Invoke-ScriptAnalyzer.md)

[Get-ScriptAnalyzerRule](Get-ScriptAnalyzerRule.md)
