---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
schema: 2.0.0
---

# Invoke-ScriptAnalyzer
## SYNOPSIS
Evaluates a script or module based on selected best practice rules

## SYNTAX

### UNNAMED_PARAMETER_SET_1
```
Invoke-ScriptAnalyzer [-Path] <String> [-CustomRulePath <String>] [-RecurseCustomRulePath]
 [-ExcludeRule <String[]>] [-IncludeRule <String[]>] [-Severity <String[]>] [-Recurse] [-SuppressedOnly] [-Fix] [-EnableExit] [-ReportSummary]
 [-Settings <String>]
```

### UNNAMED_PARAMETER_SET_2
```
Invoke-ScriptAnalyzer [-ScriptDefinition] <String> [-CustomRulePath <String>] [-RecurseCustomRulePath]
 [-ExcludeRule <String[]>] [-IncludeRule <String[]>] [-Severity <String[]>] [-Recurse] [-SuppressedOnly] [-EnableExit] [-ReportSummary]
 [-Settings <String>]
```

## DESCRIPTION
Invoke-ScriptAnalyzer evaluates a script or module files (.ps1, .psm1 and .psd1 files) based on a collection of best practice rules and returns objects
that represent rule violations. It also includes special rules to analyze DSC resources.

In each evaluation, you can run either all rules or just a specific set using the -IncludeRule parameter and also exclude rules using the -ExcludeRule parameter.
Invoke-ScriptAnalyzer comes with a set of built-in rules, but you can also use customized rules that you write in
Windows PowerShell scripts, or compile in assemblies by using C#. This is possible by using the -CustomRulePath parameter and it will then only run those custom rules, if the built-in rules should still be run, then also specify the -IncludeDefaultRules parameter. Custom rules are also supported together with the -IncludeRule and -ExcludeRule parameters. To include multiple custom rules, the -RecurseCustomRulePath parameter can be used.

To analyze your script or module, begin by using the Get-ScriptAnalyzerRule cmdlet to examine and select the rules you
want to include and/or exclude from the evaluation.

You can also include a rule in the analysis, but suppress the output of that rule for selected functions or scripts.
This feature should be used only when absolutely necessary.
To get rules that were suppressed, run Invoke-ScriptAnalyzer with the -SuppressedOnly parameter.
For instructions on suppressing a rule, see the description of the SuppressedOnly parameter.

For usage in CI systems, the -EnableExit exits the shell with an exit code equal to  the number of error records.

PSScriptAnalyzer is an open-source project.
For more information about PSScriptAnalyzer, to contribute or file an issue, see GitHub.com\PowerShell\PSScriptAnalyzer.

## EXAMPLES

### -------------------------- EXAMPLE 1 --------------------------
```
Invoke-ScriptAnalyzer -Path C:\Scripts\Get-LogData.ps1
```

This command runs all Script Analyzer rules on the Get-LogData.ps1 script.

### -------------------------- EXAMPLE 2 --------------------------
```
Invoke-ScriptAnalyzer -Path $home\Documents\WindowsPowerShell\Modules -Recurse
```

This command runs all Script Analyzer rules on all .ps1 and .psm1 files in the Modules directory and its
subdirectories.

### -------------------------- EXAMPLE 3 --------------------------
```
Invoke-ScriptAnalyzer -Path C:\Windows\System32\WindowsPowerShell\v1.0\Modules\PSDiagnostics -IncludeRule PSAvoidUsingPositionalParameters
```

This command runs only the PSAvoidUsingPositionalParameters rule on the files in the PSDiagnostics module.
You might use a command like this to find all instances of a particular rule violation while working to eliminate it.

### -------------------------- EXAMPLE 4 --------------------------
```
Invoke-ScriptAnalyzer -Path C:\ps-test\MyModule -Recurse -ExcludeRule PSAvoidUsingCmdletAliases, PSAvoidUsingInternalURLs
```

This command runs Script Analyzer on the .ps1 and .psm1 files in the MyModules directory, including the scripts in its subdirectories, with all rules except for PSAvoidUsingCmdletAliases and PSAvoidUsingInternalURLs.

### -------------------------- EXAMPLE 5 --------------------------
```
Invoke-ScriptAnalyzer -Path D:\test_scripts\Test-Script.ps1 -CustomRulePath C:\CommunityAnalyzerRules
```

This command runs Script Analyzer on Test-Script.ps1 with the standard rules and rules in the C:\CommunityAnalyzerRules path.

### -------------------------- EXAMPLE 6 --------------------------
```
$DSCError = Get-ScriptAnalyzerRule -Severity Error | Where SourceName -eq PSDSC

PS C:\>$Path = "$home\Documents\WindowsPowerShell\Modules\MyDSCModule"

PS C:\> Invoke-ScriptAnalyzerRule -Path $Path -IncludeRule $DSCError -Recurse
```

This example runs only the rules that are Error severity and have the PSDSC source name.

### -------------------------- EXAMPLE 7 --------------------------
```
function Get-Widgets
{
    [CmdletBinding()]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseSingularNouns", "")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingCmdletAliases", "", Justification="Resolution in progress.")]
    Param()

    dir $pshome
    ...
}

PS C:\> Invoke-ScriptAnalyzer -Path .\Get-Widgets.ps1

RuleName                            Severity     FileName   Line  Message
--------                            --------     --------   ----  -------
PSProvideCommentHelp                Information  ManageProf 14    The cmdlet 'Get-Widget' does not have a help comment.
                                                 iles.psm1

PS C:\> Invoke-ScriptAnalyzer -Path .\Get-Widgets.ps1 -SuppressedOnly

Rule Name                           Severity     File Name  Line  Justification
---------                           --------     ---------  ----  -------------
PSAvoidUsingCmdletAliases           Warning      ManageProf 21    Resolution in progress.
                                                 iles.psm1
PSUseSingularNouns                  Warning      ManageProf 14
                                                 iles.psm1
```

This example shows how to suppress the reporting of rule violations in a function and how to discover rule violations
that are suppressed.

The example uses the SuppressMessageAttribute attribute to suppress the PSUseSingularNouns and
PSAvoidUsingCmdletAliases rules for the Get-Widgets function in the Get-Widgets.ps1 script.
You can use this attribute
to suppress a rule for a module, script, class, function, parameter, or line.

The first command runs Script Analyzer on the script that contains the Get-Widgets function.
The output reports a rule
violation, but neither of the suppressed rules is listed, even though they are violated.

The second command uses the SuppressedOnly parameter to discover the rules that are supressed in the Get-Widgets.ps1
file.
The output reports the suppressed rules.

### -------------------------- EXAMPLE 8 --------------------------
```
# In .\ScriptAnalyzerProfile.txt
@{
    Severity = @('Error', 'Warning')
    IncludeRules = 'PSAvoid*'
    ExcludeRules = '*WriteHost'
}

PS C:\> Invoke-ScriptAnalyzer -Path $pshome\Modules\BitLocker -Profile .\ScriptAnalyzerProfile.txt
```

In this example, we create a Script Analyzer profile and save it in the ScriptAnalyzerProfile.txt file in the local
directory.

Next, we run Invoke-ScriptAnalyzer on the BitLocker module files.
The value of the Profile parameter is the path to the
Script Analyzer profile.

If you include a conflicting parameter in the Invoke-ScriptAnalyzer command, such as '-Severity Error',
Invoke-ScriptAnalyzer uses the profile value and ignores the parameter.

### -------------------------- EXAMPLE 9 --------------------------
```
Invoke-ScriptAnalyzer -ScriptDefinition "function Get-Widgets {Write-Host 'Hello'}"

RuleName                            Severity     FileName   Line  Message
--------                            --------     --------   ----  -------
PSAvoidUsingWriteHost               Warning                 1     Script
                                                                  because
                                                                  there i
                                                                  suppres
                                                                  Write-O
PSUseSingularNouns                  Warning                 1     The cmd
                                                                  noun sh
```

This command uses the ScriptDefinition parameter to analyze a function at the command line.
The function string is enclosed in quotation marks.

When you use the ScriptDefinition parameter, the FileName property of the DiagnosticRecord object is $null.

## PARAMETERS

### -Path
Specifies the path to the scripts or module to be analyzed.
Wildcard characters are supported.

Enter the path to a script (.ps1) or module file (.psm1) or to a directory that contains scripts or modules.
If the directory contains other types of files, they are ignored.

To analyze files that are not in the root directory of the specified path, use a wildcard character
(C:\Modules\MyModule\*) or the Recurse parameter.

```yaml
Type: String
Parameter Sets: UNNAMED_PARAMETER_SET_1
Aliases: PSPath

Required: True
Position: 0
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

### -CustomRulePath
Uses only the custom rules defined in the specified paths to the analysis. To still use the built-in rules, add the -IncludeDefaultRules switch.

Enter the path to a file that defines rules or a directory that contains files that define rules.
Wildcard characters are supported.
To add rules defined in subdirectories of the path, use the RecurseCustomRulePath parameter.

By default, Invoke-ScriptAnalyzer uses only rules defined in the Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll file in the PSScriptAnalyzer module.

If Invoke-ScriptAnalyzer cannot find rules in the CustomRulePath, it runs the standard rules without notice.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: CustomizedRulePath

Required: False
Position: Named
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecurseCustomRulePath
Adds rules defined in subdirectories of the CustomRulePath location.
By default, Invoke-ScriptAnalyzer uses only the custom rules defined in the specified file or directory.
To still use the built-in rules, additionally use the -IncludeDefaultRules switch.

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

### -ExcludeRule
Omits the specified rules from the Script Analyzer test.
Wildcard characters are supported.

Enter a comma-separated list of rule names, a variable that contains rule names, or a command that gets rule names.
You can also specify a list of excluded rules in a Script Analyzer profile file.
You can exclude standard rules and rules in a custom rule path.

When you exclude a rule, the rule does not run on any of the files in the path.
To exclude a rule on a particular line, parameter, function, script, or class, adjust the Path parameter or suppress the rule.
For information about suppressing a rule, see the examples.

If a rule is specified in both the ExcludeRule and IncludeRule collections, the rule is excluded.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rules are included.
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeDefaultRules
Invoke default rules along with Custom rules

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

### -IncludeRule
Runs only the specified rules in the Script Analyzer test.
By default, PSScriptAnalyzer runs all rules.

Enter a comma-separated list of rule names, a variable that contains rule names, or a command that gets rule names.
Wildcard characters are supported.
You can also specify rule names in a Script Analyzer profile file.

When you use the CustomizedRulePath parameter, you can use this parameter to include standard rules and rules in the
custom rule paths.

If a rule is specified in both the ExcludeRule and IncludeRule collections, the rule is excluded.

Also, Severity takes precedence over IncludeRule.
For example, if Severity is Error, you cannot use IncludeRule to include a Warning rule.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rules are included.
Accept pipeline input: False
Accept wildcard characters: False
```

### -Severity
After running Script Analyzer with all rules, this parameter selects rule violations with the specified severity.

Valid values are: Error, Warning, and Information.
You can specify one ore more severity values.

Because this parameter filters the rules only after running with all rules, it is not an efficient filter.
To filter rules efficiently, use Get-ScriptAnalyzer rule to get the rules you want to run or exclude and
then use the ExcludeRule or IncludeRule parameters.

Also, Severity takes precedence over IncludeRule.
For example, if Severity is Error, you cannot use IncludeRule to include a Warning rule.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rule violations
Accept pipeline input: False
Accept wildcard characters: False
```

### -Recurse
Runs Script Analyzer on the files in the Path directory and all subdirectories recursively.

Recurse applies only to the Path parameter value.
To search the CustomRulePath recursively, use the RecurseCustomRulePath parameter.

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

### -SuppressedOnly
Returns rules that are suppressed, instead of analyzing the files in the path.

When you used SuppressedOnly, Invoke-ScriptAnalyzer returns a SuppressedRecord object
(Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SuppressedRecord).

To suppress a rule, use the SuppressMessageAttribute.
For help, see the examples.

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

### -Fix
Fixes certain warnings which contain a fix in their DiagnosticRecord.

When you used Fix, Invoke-ScriptAnalyzer runs as usual but will apply the fixes before running the analysis.
Please make sure that you have a backup of your files when using this switch.
It tries to preserve the file encoding but there are still some cases where the encoding can change.

```yaml
Type: SwitchParameter
Parameter Sets: UNNAMED_PARAMETER_SET_1
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -EnableExit
Exits PowerShell and returns an exit code equal to the number of error records. This can be useful in CI systems.

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

### -ReportSummary
Writes a report summary of the found warnings to the host.

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

### -Settings
File path that contains user profile or hash table for ScriptAnalyzer

Runs Invoke-ScriptAnalyzer with the parameters and values specified in a Script Analyzer profile file or hash table

If the path, the file's or hashtable's content are invalid, it is ignored.
The parameters and values in the profile take precedence over the same parameter and values specified at the command line.

A Script Analyzer profile file is a text file that contains a hash table with one or more of the following keys:

-- CustomRulePath
-- ExcludeRules
-- IncludeDefaultRules
-- IncludeRules
-- RecurseCustomRulePath
-- Rules
-- Severity

The keys and values in the profile are interpreted as if they were standard parameters and parameter values of Invoke-ScriptAnalyzer.

To specify a single value, enclose the value in quotation marks.
For example:

    @{ Severity = 'Error'}

To specify multiple values, enclose the values in an array.
For example:

    @{ Severity = 'Error', 'Warning'}

A more sophisticated example is:

    @{
        CustomRulePath='path\to\CustomRuleModule.psm1'
        IncludeDefaultRules=$true
        ExcludeRules = @(
            'PSAvoidUsingWriteHost',
            'MyCustomRuleName'
        )
    }

```yaml
Type: Object
Parameter Sets: (All)
Aliases: Profile

Required: False
Position: Named
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

### -ScriptDefinition
Runs Invoke-ScriptAnalyzer on commands, functions, or expressions in a string.
You can use this feature to analyze statements, expressions, and functions, independent of their script context.

Unlike ScriptBlock parameters, the ScriptDefinition parameter requires a string value.

```yaml
Type: String
Parameter Sets: UNNAMED_PARAMETER_SET_2
Aliases:

Required: True
Position: 0
Default value:
Accept pipeline input: False
Accept wildcard characters: False
```

### -SaveDscDependency
Resolve DSC resource dependency

Whenever Invoke-ScriptAnalyzer is run on a script having the dynamic keyword "Import-DSCResource -ModuleName <somemodule>", if <somemodule> is not present in any of the PSModulePath, Invoke-ScriptAnalyzer gives parse error. This error is caused by the powershell parser not being able to find the symbol for <somemodule>. If Invoke-ScriptAnalyzer finds the module in the PowerShell Gallery (www.powershellgallery.com) then it downloads the missing module to a temp path. The temp path is then added to PSModulePath only for duration of the scan. The temp location can be found in $LOCALAPPDATA/PSScriptAnalyzer/TempModuleDir.

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


## INPUTS

### None
You cannot pipe input to this cmdlet.

## OUTPUTS

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord
By default, Invoke-ScriptAnalyzer returns one DiagnosticRecord object to report a rule violation.

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SuppressedRecord
If you use the SuppressedOnly parameter, Invoke-ScriptAnalyzer instead returns a SuppressedRecord object.

## NOTES

## RELATED LINKS

[Get-ScriptAnalyzerRule]()

[PSScriptAnalyzer on GitHub](https://github.com/PowerShell/PSScriptAnalyzer)
