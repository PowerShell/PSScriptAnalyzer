---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
Module Name: PSScriptAnalyzer
ms.date: 10/07/2021
online version: https://docs.microsoft.com/powershell/module/psscriptanalyzer/get-scriptanalyzerrule?view=ps-modules&wt.mc_id=ps-gethelp
schema: 2.0.0
---

# Get-ScriptAnalyzerRule

## SYNOPSIS
Gets the script analyzer rules on the local computer.

## SYNTAX

```
Get-ScriptAnalyzerRule [[-Name] <string[]>] [-CustomRulePath <string[]>] [-RecurseCustomRulePath]
 [-Severity <string[]>] [<CommonParameters>]
```

## DESCRIPTION

Gets the script analyzer rules on the local computer. You can select rules by Name, Severity,
Source, or SourceType, or even particular words in the rule description.

Use this cmdlet to create collections of rules to include and exclude when running the
`Invoke-ScriptAnalyzer` cmdlet.

To get information about the rules, see the value of the Description property of each rule.

The PSScriptAnalyzer module tests the PowerShell code in a script, module, or DSC resource to
determine if it fulfils best practice standards.

## EXAMPLES

### EXAMPLE 1 - Get all Script Analyzer rules on the local computer

```powershell
Get-ScriptAnalyzerRule
```

### EXAMPLE 2 - Gets only rules with the Error severity

```powershell
Get-ScriptAnalyzerRule -Severity Error
```

### EXAMPLE 3 - Run only the DSC rules with the Error severity

This example runs only the DSC rules with the Error severity on the files in the **MyDSCModule**
module.

```powershell
$DSCError = Get-ScriptAnalyzerRule -Severity Error | Where-Object SourceName -eq PSDSC
$Path = "$home\Documents\WindowsPowerShell\Modules\MyDSCModule\*"
Invoke-ScriptAnalyzerRule -Path $Path -IncludeRule $DSCError -Recurse
```

Using the **IncludeRule** parameter of `Invoke-ScriptAnalyzerRule` is more efficient than using its
**Severity** parameter, which is applied only after using all rules to analyze all module files.

### EXAMPLE 4 - Get rules by name and severity

This example gets rules with "Parameter" or "Alias" in the name that generate an Error or Warning.
You can use this set of rules to test the parameters of your script or module.

```powershell
$TestParameters = Get-ScriptAnalyzerRule -Severity Error, Warning -Name *Parameter*, *Alias*
```

### EXAMPLE 5 - Get custom rules

This example gets the standard rules and the rules in the **VeryStrictRules** and
**ExtremelyStrictRules** modules. The command uses the **RecurseCustomRulePath** parameter to get
rules defined in subdirectories of the matching paths.

```powershell
Get-ScriptAnalyzerRule -CustomRulePath $home\Documents\WindowsPowerShell\Modules\*StrictRules -RecurseCustomRulePath
```

## PARAMETERS

### -CustomRulePath

By default, PSScriptAnalyzer gets only the standard rules specified in the
`Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll` file in the module. Use this
parameter to get the custom Script Analyzer rules in the specified path and the standard Script
Analyzer rules.

Enter the path to a .NET assembly or module that contains Script Analyzer rules. You can enter only
one value, but wildcards are supported. To get rules in subdirectories of the path, use the
**RecurseCustomRulePath** parameter.

You can create custom rules by using a custom .NET assembly or a PowerShell module, such as the
[Community Analyzer Rules](https://github.com/PowerShell/PSScriptAnalyzer/blob/development/Tests/Engine/CommunityAnalyzerRules/CommunityAnalyzerRules.psm1)
in the GitHub repository.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: CustomizedRulePath

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Name

Gets only rules with the specified names or name patterns. Wildcards are supported. If you list
multiple names or patterns, it gets all rules that match any of the name patterns.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rules
Accept pipeline input: False
Accept wildcard characters: True
```

### -RecurseCustomRulePath

Searches the **CustomRulePath** location recursively to add rules defined in files in subdirectories
of the path. By default, `Get-ScriptAnalyzerRule` adds only the custom rules in the specified path.

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

### -Severity

Gets only rules with the specified severity values. Valid values are:

- Information
- Warning
- Error

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rules
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

You cannot pipe input to this cmdlet.

## OUTPUTS

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleInfo

The **RuleInfo** object is a custom object created specifically for Script Analyzer.

## NOTES

## RELATED LINKS

[Invoke-ScriptAnalyzer](Invoke-ScriptAnalyzer.md)

[PSScriptAnalyzer on GitHub](https://github.com/PowerShell/PSScriptAnalyzer)
