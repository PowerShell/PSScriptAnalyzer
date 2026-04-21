---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
Module Name: PSScriptAnalyzer
ms.date: 04/17/2026
schema: 2.0.0
---

# Test-ScriptAnalyzerSettingsFile

## SYNOPSIS
Validates a PSScriptAnalyzer settings file as a self-contained unit.

## SYNTAX

```
Test-ScriptAnalyzerSettingsFile [-Path] <string> [-Quiet] [<CommonParameters>]
```

## DESCRIPTION

The `Test-ScriptAnalyzerSettingsFile` cmdlet validates a PSScriptAnalyzer settings file as a
self-contained unit. It reads `CustomRulePath`, `RecurseCustomRulePath`, and `IncludeDefaultRules`
directly from the file so that validation reflects the same rule set `Invoke-ScriptAnalyzer` would
see when given the same file.

The cmdlet verifies that:

- The file can be parsed as a PowerShell data file.
- All rule names referenced in `IncludeRules`, `ExcludeRules`, and `Rules` correspond to known
  rules (wildcard patterns are skipped).
- All `Severity` values are valid.
- Rule option names in the `Rules` section correspond to actual configurable properties.
- Rule option values that are constrained to a set of choices contain a valid value.

By default, when problems are found the cmdlet outputs a `DiagnosticRecord` for each one, with the
source extent pointing to the offending text in the file. This is the same object type returned by
`Invoke-ScriptAnalyzer`, so existing formatting and tooling works out of the box. When the file is
valid, no output is produced.

When `-Quiet` is specified the cmdlet returns only `$true` or `$false` and suppresses all
diagnostic output.

## EXAMPLES

### EXAMPLE 1 - Validate a settings file

```powershell
Test-ScriptAnalyzerSettingsFile -Path ./PSScriptAnalyzerSettings.psd1
```

Outputs a `DiagnosticRecord` for each problem found, with line and column information. Produces no
output when the file is valid.

### EXAMPLE 2 - Validate quietly in a conditional

```powershell
if (Test-ScriptAnalyzerSettingsFile -Path ./PSScriptAnalyzerSettings.psd1 -Quiet) {
    Invoke-ScriptAnalyzer -Path ./src -Settings ./PSScriptAnalyzerSettings.psd1
}
```

Returns `$true` or `$false` without producing diagnostic output.

### EXAMPLE 3 - Validate a file that uses custom rules

```powershell
# Settings.psd1 contains CustomRulePath and IncludeDefaultRules keys.
# The cmdlet reads those from the file directly — no extra parameters needed.
Test-ScriptAnalyzerSettingsFile -Path ./Settings.psd1
```

Validates rule names against both built-in and custom rules as specified in the settings file.

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

Suppresses diagnostic output and returns only `$true` or `$false`. Without this switch the cmdlet
outputs a `DiagnosticRecord` for each problem found and produces no output when the file is valid.

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

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord

Without `-Quiet`, a `DiagnosticRecord` is output for each problem found. Each record includes the
error message, the source extent (file, line and column), a severity, and the rule name
`Test-ScriptAnalyzerSettingsFile`. No output is produced when the file is valid.

### System.Boolean

With `-Quiet`, returns `$true` when the file is valid and `$false` otherwise.

## NOTES

The cmdlet reads `CustomRulePath`, `RecurseCustomRulePath`, and `IncludeDefaultRules` from the
settings file so it validates rule names against the same set of rules that `Invoke-ScriptAnalyzer`
would load. This means the settings file is validated as a self-contained unit without requiring
extra command-line parameters.

Note: Relative paths in `CustomRulePath` are resolved from the caller's current working directory,
not from the location of the settings file. This matches `Invoke-ScriptAnalyzer` behaviour.

The `DiagnosticRecord` objects use the same type as `Invoke-ScriptAnalyzer`, so they benefit from
the same default formatting and can be piped to the same downstream tooling.

## RELATED LINKS

[New-ScriptAnalyzerSettingsFile](New-ScriptAnalyzerSettingsFile.md)

[Invoke-ScriptAnalyzer](Invoke-ScriptAnalyzer.md)

[Get-ScriptAnalyzerRule](Get-ScriptAnalyzerRule.md)
