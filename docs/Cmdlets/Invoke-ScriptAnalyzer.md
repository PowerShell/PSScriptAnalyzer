---
external help file: Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml
Module Name: PSScriptAnalyzer
ms.date: 10/07/2021
online version: https://learn.microsoft.com/powershell/module/psscriptanalyzer/invoke-scriptanalyzer?view=ps-modules&wt.mc_id=ps-gethelp
schema: 2.0.0
---

# Invoke-ScriptAnalyzer

## SYNOPSIS
Evaluates a script or module based on selected best practice rules

## SYNTAX

### Path_SuppressedOnly (Default)

```
Invoke-ScriptAnalyzer [-Path] <string> [-CustomRulePath <string[]>] [-RecurseCustomRulePath]
 [-IncludeDefaultRules] [-ExcludeRule <string[]>] [-IncludeRule <string[]>] [-Severity <string[]>]
 [-Recurse] [-SuppressedOnly] [-Fix] [-EnableExit] [-Settings <Object>] [-SaveDscDependency]
 [-ReportSummary] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Path_IncludeSuppressed

```
Invoke-ScriptAnalyzer [-Path] <string> -IncludeSuppressed [-CustomRulePath <string[]>]
 [-RecurseCustomRulePath] [-IncludeDefaultRules] [-ExcludeRule <string[]>]
 [-IncludeRule <string[]>] [-Severity <string[]>] [-Recurse] [-Fix] [-EnableExit]
 [-Settings <Object>] [-SaveDscDependency] [-ReportSummary] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### ScriptDefinition_IncludeSuppressed

```
Invoke-ScriptAnalyzer [-ScriptDefinition] <string> -IncludeSuppressed [-CustomRulePath <string[]>]
 [-RecurseCustomRulePath] [-IncludeDefaultRules] [-ExcludeRule <string[]>]
 [-IncludeRule <string[]>] [-Severity <string[]>] [-Recurse] [-EnableExit] [-Settings <Object>]
 [-SaveDscDependency] [-ReportSummary] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ScriptDefinition_SuppressedOnly

```
Invoke-ScriptAnalyzer [-ScriptDefinition] <string> [-CustomRulePath <string[]>]
 [-RecurseCustomRulePath] [-IncludeDefaultRules] [-ExcludeRule <string[]>]
 [-IncludeRule <string[]>] [-Severity <string[]>] [-Recurse] [-SuppressedOnly] [-EnableExit]
 [-Settings <Object>] [-SaveDscDependency] [-ReportSummary] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

`Invoke-ScriptAnalyzer` evaluates scripts or module files (`.ps1`, `.psm1`, and `.psd1` files) based
on a collection of best practice rules and returns objects that represent rule violations. It also
includes special rules to analyze DSC resources.

`Invoke-ScriptAnalyzer` comes with a set of built-in rules. By default, it uses all rules. You can
use the **IncludeRule** and **ExcludeRule** parameters to select the rules you want. You can use the
`Get-ScriptAnalyzerRule` cmdlet to examine and select the rules you want to include or exclude from
the evaluation.

You can also use customized rules that you write in PowerShell scripts, or compile in assemblies
using C#. Custom rules can also be selected using the **IncludeRule** and **ExcludeRule**
parameters.

You can also include a rule in the analysis, but suppress the output of that rule for selected
functions or scripts. This feature should be used only when necessary. To get rules that were
suppressed, run `Invoke-ScriptAnalyzer` with the **SuppressedOnly** parameter.

For usage in CI systems, the **EnableExit** exits the shell with an exit code equal to the number of
error records.

## EXAMPLES

### EXAMPLE 1 - Run all Script Analyzer rules on a script

```powershell
Invoke-ScriptAnalyzer -Path C:\Scripts\Get-LogData.ps1
```

### EXAMPLE 2 - Run all Script Analyzer rules on all files in the Modules directory

This example runs all Script Analyzer rules on all `.ps1` and `.psm1` files in your user-based
`Modules` directory and its subdirectories.

```powershell
Invoke-ScriptAnalyzer -Path $home\Documents\WindowsPowerShell\Modules -Recurse
```

### EXAMPLE 3 - Run a single rule on a module

This example runs only the **PSAvoidUsingPositionalParameters** rule on the files in the
`PSDiagnostics` module folder. You can use a command like this to find all instances of a particular
rule violation.

```powershell
Invoke-ScriptAnalyzer -Path C:\Windows\System32\WindowsPowerShell\v1.0\Modules\PSDiagnostics -IncludeRule PSAvoidUsingPositionalParameters
```

### EXAMPLE 4 - Run all rules except two on your modules

This example runs all rules except for **PSAvoidUsingCmdletAliases** and
**PSAvoidUsingInternalURLs** on the `.ps1` and `.psm1` files in the `MyModules` directory and in its
subdirectories.

```powershell
Invoke-ScriptAnalyzer -Path C:\ps-test\MyModule -Recurse -ExcludeRule PSAvoidUsingCmdletAliases, PSAvoidUsingInternalURLs
```

### EXAMPLE 5 - Run Script Analyzer with custom rules

This example runs Script Analyzer on `Test-Script.ps1` with the standard rules and rules in the
`C:\CommunityAnalyzerRules` path.

```powershell
Invoke-ScriptAnalyzer -Path D:\test_scripts\Test-Script.ps1 -CustomRulePath C:\CommunityAnalyzerRules -IncludeDefaultRules
```

### EXAMPLE 6 - Run only the rules that are Error severity and have the PSDSC source name

```powershell
$DSCError = Get-ScriptAnalyzerRule -Severity Error | Where SourceName -eq PSDSC
$Path = "$home\Documents\WindowsPowerShell\Modules\MyDSCModule"
Invoke-ScriptAnalyzerRule -Path $Path -IncludeRule $DSCError -Recurse
```

### EXAMPLE 7 - Suppressing rule violations

This example shows how to suppress the reporting of rule violations in a function and how to
discover rule violations that are suppressed.

The example uses the `SuppressMessageAttribute` attribute to suppress the **PSUseSingularNouns** and
**PSAvoidUsingCmdletAliases** rules for the `Get-Widgets` function in the `Get-Widgets.ps1` script.
You can use this attribute to suppress a rule for a module, script, class, function, parameter, or
line.

The first command runs Script Analyzer on the script file containing the function. The output
reports a rule violation. Even though more rules are violated, neither suppressed rule is reported.

```powershell
function Get-Widgets
{
    [CmdletBinding()]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseSingularNouns", "")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingCmdletAliases", "", Justification="Resolution in progress.")]
    Param()

    dir $pshome
    ...
}

Invoke-ScriptAnalyzer -Path .\Get-Widgets.ps1
```

```Output
RuleName                            Severity     FileName   Line  Message
--------                            --------     --------   ----  -------
PSProvideCommentHelp                Information  ManageProf 14    The cmdlet 'Get-Widget' does not have a help comment.
                                                 iles.psm1
```

```powershell
Invoke-ScriptAnalyzer -Path .\Get-Widgets.ps1 -SuppressedOnly
```

```Output
Rule Name                           Severity     File Name  Line  Justification
---------                           --------     ---------  ----  -------------
PSAvoidUsingCmdletAliases           Warning      ManageProf 21    Resolution in progress.
                                                 iles.psm1
PSUseSingularNouns                  Warning      ManageProf 14
                                                 iles.psm1
```

The second command uses the **SuppressedOnly** parameter to report violations of the rules that are
suppressed script file.

### EXAMPLE 8 - Analyze script files using a profile definition

In this example, we create a Script Analyzer profile and save it in the `ScriptAnalyzerProfile.txt`
file in the current directory. We run `Invoke-ScriptAnalyzer` on the **BitLocker** module files. The
value of the **Profile** parameter is the path to the Script Analyzer profile.

```powershell
# In .\ScriptAnalyzerProfile.txt
@{
    Severity = @('Error', 'Warning')
    IncludeRules = 'PSAvoid*'
    ExcludeRules = '*WriteHost'
}

Invoke-ScriptAnalyzer -Path $pshome\Modules\BitLocker -Settings .\ScriptAnalyzerProfile.txt
```

If you include a conflicting parameter in the `Invoke-ScriptAnalyzer` command, such as
`-Severity Error`, the cmdlet uses the profile value and ignores the parameter.

### EXAMPLE 9 - Analyze a script stored as a string

This example uses the **ScriptDefinition** parameter to analyze a function at the command line. The
function string is enclosed in quotation marks.

```powershell
Invoke-ScriptAnalyzer -ScriptDefinition "function Get-Widgets {Write-Host 'Hello'}"
```

```Output
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

When you use the **ScriptDefinition** parameter, the **FileName** property of the
**DiagnosticRecord** object is `$null`.

## PARAMETERS

### -CustomRulePath

Enter the path to a file that defines rules or a directory that contains files that define rules.
Wildcard characters are supported. When **CustomRulePath** is specified, only the custom rules found
in the specified paths are used for the analysis. If `Invoke-ScriptAnalyzer` cannot find rules in
the , it runs the standard rules without notice.

To add rules defined in subdirectories of the path, use the **RecurseCustomRulePath** parameter. To
include the built-in rules, add the **IncludeDefaultRules** parameter.

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

### -EnableExit

On completion of the analysis, this parameter exits the PowerShell sessions and returns an exit code
equal to the number of error records. This can be useful in continuous integration (CI) pipeline.

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

### -ExcludeRule

Omits the specified rules from the Script Analyzer test. Wildcard characters are supported.

Enter a comma-separated list of rule names, a variable that contains rule names, or a command that
gets rule names. You can also specify a list of excluded rules in a Script Analyzer profile file.
You can exclude standard rules and rules in a custom rule path.

When you exclude a rule, the rule does not run on any of the files in the path. To exclude a rule on
a particular line, parameter, function, script, or class, adjust the Path parameter or suppress the
rule. For information about suppressing a rule, see the examples.

If a rule is specified in both the **ExcludeRule** and **IncludeRule** collections, the rule is
excluded.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rules are included.
Accept pipeline input: False
Accept wildcard characters: True
```

### -Fix

Fixes certain warnings that contain a fix in their **DiagnosticRecord**.

When you used **Fix**, `Invoke-ScriptAnalyzer` applies the fixes before running the analysis. Make
sure that you have a backup of your files when using this parameter. It tries to preserve the file
encoding but there are still some cases where the encoding can change.

```yaml
Type: SwitchParameter
Parameter Sets: Path_SuppressedOnly, Path_IncludeSuppressed
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeDefaultRules

Invoke default rules along with Custom rules.

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

Runs only the specified rules in the Script Analyzer test. By default, PSScriptAnalyzer runs all
rules.

Enter a comma-separated list of rule names, a variable that contains rule names, or a command that
gets rule names. Wildcard characters are supported. You can also specify rule names in a Script
Analyzer profile file.

When you use the **CustomizedRulePath** parameter, you can use this parameter to include standard
rules and rules in the custom rule paths.

If a rule is specified in both the **ExcludeRule** and **IncludeRule** collections, the rule is
excluded.

The **Severity** parameter takes precedence over **IncludeRule**. For example, if **Severity** is
`Error`, you cannot use **IncludeRule** to include a `Warning` rule.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: All rules are included.
Accept pipeline input: False
Accept wildcard characters: True
```

### -IncludeSuppressed

Include suppressed diagnostics in output.

```yaml
Type: SwitchParameter
Parameter Sets: Path_IncludeSuppressed, ScriptDefinition_IncludeSuppressed
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

Specifies the path to the scripts or module to be analyzed. Wildcard characters are supported.

Enter the path to a script (`.ps1`) or module file (`.psm1`) or to a directory that contains scripts
or modules. If the directory contains other types of files, they are ignored.

To analyze files that are not in the root directory of the specified path, use a wildcard character
(`C:\Modules\MyModule\*`) or the **Recurse** parameter.

```yaml
Type: String
Parameter Sets: Path_SuppressedOnly, Path_IncludeSuppressed
Aliases: PSPath

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: True
```

### -Recurse

Runs Script Analyzer on the files in the **Path** directory and all subdirectories recursively.

Recurse applies only to the Path parameter value. To search the **CustomRulePath** recursively, use
the **RecurseCustomRulePath** parameter.

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

### -RecurseCustomRulePath

Adds rules defined in subdirectories of the **CustomRulePath** location. By default,
`Invoke-ScriptAnalyzer` uses only the custom rules defined in the specified file or directory. To
include the built-in rules, use the **IncludeDefaultRules** parameter.

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

Write a summary of the violations found to the host.

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

### -SaveDscDependency

Resolve DSC resource dependencies.

When `Invoke-ScriptAnalyzer` is run with this parameter, it looks for instances of
`Import-DSCResource -ModuleName <somemodule>`. If `<somemodule>` is cannot be found by searching the
`$env:PSModulePath`, `Invoke-ScriptAnalyzer` returns parse error. This error is caused by the
PowerShell parser not being able to find the symbol for `<somemodule>`.

If `Invoke-ScriptAnalyzer` finds the module in the PowerShell Gallery, it downloads the missing
module to a temp path. The temp path is then added to `$env:PSModulePath` for duration of the scan.
The temp location can be found in `$LOCALAPPDATA/PSScriptAnalyzer/TempModuleDir`.

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

### -ScriptDefinition

Runs the analysis on commands, functions, or expressions in a string. You can use this feature to
analyze statements, expressions, and functions, independent of their script context.

```yaml
Type: String
Parameter Sets: ScriptDefinition_IncludeSuppressed, ScriptDefinition_SuppressedOnly
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Settings

A path to a file containing a user-defined profile or a hashtable object containing settings for
ScriptAnalyzer.

Runs `Invoke-ScriptAnalyzer` with the parameters and values specified in the file or hashtable.

If the path or the content of the file or hashtable is invalid, it is ignored. The parameters and
values in the profile take precedence over the same parameter and values specified at the command
line.

A Script Analyzer profile file is a text file that contains a hashtable with one or more of the
following keys:

- CustomRulePath
- ExcludeRules
- IncludeDefaultRules
- IncludeRules
- RecurseCustomRulePath
- Rules
- Severity

The keys and values in the profile are interpreted as if they were standard parameters and values of
`Invoke-ScriptAnalyzer`, similar to splatting. For more information, see
[about_Splatting](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_splatting).

```yaml
Type: Object
Parameter Sets: (All)
Aliases: Profile

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Severity

After running Script Analyzer with all rules, this parameter selects rule violations with the
specified severity.

Valid values are:

- Error
- Warning
- Information.

You can specify one ore more severity values.

The parameter filters the rules violations only after running all rules. To filter rules
efficiently, use `Get-ScriptAnalyzerRule` to select the rules you want to run.

The **Severity** parameter takes precedence over **IncludeRule**. For example, if **Severity** is
`Error`, you cannot use **IncludeRule** to include a `Warning` rule.

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

### -SuppressedOnly

Returns violations only for rules that are suppressed.

Returns a **SuppressedRecord** object
(**Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SuppressedRecord**).

To suppress a rule, use the **SuppressMessageAttribute**. For help, see the examples.

```yaml
Type: SwitchParameter
Parameter Sets: Path_SuppressedOnly, ScriptDefinition_SuppressedOnly
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

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

You cannot pipe input to this cmdlet.

## OUTPUTS

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord

By default, `Invoke-ScriptAnalyzer` returns one **DiagnosticRecord** object for each rule violation.

### Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SuppressedRecord

If you use the **SuppressedOnly** parameter, `Invoke-ScriptAnalyzer` instead returns a
**SuppressedRecord** objects.

## NOTES

## RELATED LINKS

[Get-ScriptAnalyzerRule](Get-ScriptAnalyzerRule.md)

[PSScriptAnalyzer on GitHub](https://github.com/PowerShell/PSScriptAnalyzer)
