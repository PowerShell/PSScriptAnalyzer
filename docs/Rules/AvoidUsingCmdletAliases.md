---
description: Avoid Using Cmdlet Aliases or omitting the 'Get-' prefix.
ms.date: 06/28/2023
ms.topic: reference
title: AvoidUsingCmdletAliases
---
# AvoidUsingCmdletAliases

**Severity Level: Warning**

## Description

An alias is an alternate name or nickname for a cmdlet or for a command element, such as a function,
script, file, or executable file. You can use the alias instead of the command name in any
PowerShell commands.

There are also implicit aliases. When PowerShell cannot find the cmdlet name, it will try to append
`Get-` to the command as a last resort. Therefore using the command `verb` will execute `Get-Verb`.

Every PowerShell author learns the actual command names, but different authors learn and use
different aliases. Aliases can make code difficult to read, understand and impact availability.

Using the full command name makes it easier to maintain your scripts in the the future.

Using the full command names also allows for syntax highlighting in sites and applications like
GitHub and Visual Studio Code.

## How to Fix

Use the full cmdlet name and not an alias.

## Alias Allowlist

To prevent `PSScriptAnalyzer` from flagging your preferred aliases, create an allowlist of the
aliases in your settings file and point `PSScriptAnalyzer` to use the settings file. For example, to
disable `PSScriptAnalyzer` from flagging `cd`, which is an alias of `Set-Location`, set the settings
file content to the following.

```powershell
# PSScriptAnalyzerSettings.psd1

@{
    'Rules' = @{
        'PSAvoidUsingCmdletAliases' = @{
            'allowlist' = @('cd')
        }
    }
}
```

## Example

### Wrong

```powershell
gps | Where-Object {$_.WorkingSet -gt 20000000}
```

### Correct

```powershell
Get-Process | Where-Object {$_.WorkingSet -gt 20000000}
```
