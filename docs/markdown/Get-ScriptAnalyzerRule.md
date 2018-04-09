---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
schema: 2.0.0
---

# Get-ScriptAnalyzerRule
## SYNOPSIS
Gets the script analyzer rules on the local computer.

## SYNTAX

```
Get-ScriptAnalyzerRule [-CustomRulePath <String>] [-RecurseCustomRulePath] [-Name <String[]>]
 [-Severity <String[]>]
```

## DESCRIPTION
Gets the script analyzer rules on the local computer.
You can select rules by Name, Severity, Source, or SourceType, or even particular words in the rule description.

Use this cmdlet to create collections of rules to include and exclude when running the Invoke-ScriptAnalyzer cmdlet.

To get information about the rules, see the value of the Description property of each rule.

The PSScriptAnalyzer module tests the Windows PowerShell code in a script, module, or DSC resource to determine whether, and to what extent, it fulfils best practice standards.

PSScriptAnalyzer is an open-source project.
For more information about PSScriptAnalyzer, to contribute or file an issue, see GitHub.com\PowerShell\PSScriptAnalyzer.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Get-ScriptAnalyzerRule
```

This command gets all Script Analyzer rules on the local computer.

### -------------------------- EXAMPLE 2 --------------------------
```
Get-ScriptAnalyzerRule -Severity Error
```

This command gets only rules with the Error severity.

### -------------------------- EXAMPLE 3 --------------------------
```
$DSCError = Get-ScriptAnalyzerRule -Severity Error | Where SourceName -eq PSDSC

PS C:\>$Path = "$home\Documents\WindowsPowerShell\Modules\MyDSCModule\*"

PS C:\> Invoke-ScriptAnalyzerRule -Path $Path -IncludeRule $DSCError -Recurse
```

This example runs only the DSC rules with the Error severity on the files in the MyDSCModule module.

Using the IncludeRule parameter of Invoke-ScriptAnalyzerRule is much more efficient than using its Severity parameter, which is applied only after using all rules to analyze all module files.

### -------------------------- EXAMPLE 4 --------------------------
```
$TestParameters = Get-ScriptAnalyzerRule -Severity Error, Warning -Name *Parameter*, *Alias*
```

This command gets rules with "Parameter" or "Alias" in the name that generate an Error or Warning.
Use this set of rules to test the parameters of your script or module.

### -------------------------- EXAMPLE 5 --------------------------
```
Get-ScriptAnalyzerRule -CustomRulePath $home\Documents\WindowsPowerShell\Modules\*StrictRules -RecurseCustomRulePath
```

This command gets the standard rules and the rules in the VeryStrictRules and ExtremelyStrictRules modules.
The command uses the RecurseCustomRulePath parameter to get rules defined in subdirectories of the matching paths.

## PARAMETERS

### -CustomRulePath
Gets the Script Analyzer rules in the specified path in addition to the standard Script Analyzer rules.
By default, PSScriptAnalyzer gets only the standard rules specified in the Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll file in the module.

Enter the path to a .NET assembly or module that contains Script Analyzer rules.
You can enter only one value, but wildcards are supported.
To get rules in subdirectories of the path, use the RecurseCustomRulePath parameter.

You can create custom rules by using a custom .NET assembly or a Windows PowerShell module, such as the Community Analyzer Rules in
https://github.com/PowerShell/PSScriptAnalyzer/blob/development/Tests/Engine/CommunityAnalyzerRules/CommunityAnalyzerRules.psm1.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: CustomizedRulePath

Required: False
Position: Named
Default value: The rules in Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll.
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecurseCustomRulePath
Searches the CustomRulePath location recursively to add rules defined in files in subdirectories of the path.
By default, Get-ScriptAnalyzerRule adds only the custom rules in the specified path.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Gets only rules with the specified names or name patterns.
Wildcards are supported.
If you list multiple names or patterns, it gets rules that match any of the name patterns, as though the name patterns were joined by an OR.

By default, Get-ScriptAnalyzerRule gets all rules.

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

### -Severity
Gets only rules with the specified severity values.
Valid values are Information, Warning, and Error.
By default, Get-ScriptAnalyzerRule gets all rules.

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

## INPUTS

### None
You cannot pipe input to this cmdlet.

## OUTPUTS

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleInfo
The RuleInfo object is a custom object created especially for Script Analyzer. It is not documented on MSDN.

## NOTES

## RELATED LINKS

[Invoke-ScriptAnalyzer]()

[PSScriptAnalyzer on GitHub](https://github.com/PowerShell/PSScriptAnalyzer)
