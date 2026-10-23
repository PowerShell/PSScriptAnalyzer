---
description: Use fully qualified module names when calling cmdlets and functions
ms.date: 09/22/2026
ms.topic: reference
title: UseFullyQualifiedCmdletNames
---
# UseFullyQualifiedCmdletNames

**Severity Level: Warning**

**Default state: Disabled**

## Description

PowerShell resolves a command name against the commands that are available in the session. A script
that calls `Get-Process` instead of `Microsoft.PowerShell.Management\Get-Process` binds to whichever
alias, function, or cmdlet currently owns that name. A module that exports the same name, or an
alias or function defined in the session, can therefore change which command the script runs.

This rule detects calls to cmdlets, module functions, and aliases that aren't qualified with the
module that provides them, and suggests the fully qualified `ModuleName\CommandName` replacement.
Qualifying a call also tells PowerShell which module to load, so a script doesn't depend on the
order in which commands happen to be available.

The rule doesn't flag native commands and external applications, functions declared in the analyzed
script, commands that don't resolve to a module, calls that already use the fully qualified form, or
variables and string literals that contain text that looks like a command name.

## Example

### Noncompliant

```powershell
# Unqualified cmdlet calls
Get-Command
Write-Host 'Hello World'
Get-ChildItem -Path C:\temp

# Aliases
gci C:\temp
ls -Force
```

### Compliant

```powershell
# Fully qualified cmdlet calls
Microsoft.PowerShell.Core\Get-Command
Microsoft.PowerShell.Utility\Write-Host 'Hello World'
Microsoft.PowerShell.Management\Get-ChildItem -Path C:\temp

# The cmdlets that the aliases resolve to
Microsoft.PowerShell.Management\Get-ChildItem C:\temp
Microsoft.PowerShell.Management\Get-ChildItem -Force
```

## Configure rule

```powershell
Rules = @{
    PSUseFullyQualifiedCmdletNames = @{
        Enable = $true
        IgnoredModules = @(
            'Microsoft.PowerShell.Management'
            'Microsoft.PowerShell.Utility'
        )
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### IgnoredModules

This parameter specifies the modules whose commands the rule doesn't flag. It accepts an array of
module-name strings, which are matched without regard to case. The default value is `@()`.

## See also

- [about_Command_Precedence][01]
- [about_Modules][02]

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_command_precedence
[02]: /powershell/module/microsoft.powershell.core/about/about_modules
