# AvoidUsingCmdletAliases

**Severity Level: Warning**

## Description

An alias is an alternate name or nickname for a CMDLet or for a command element, such as a function, script, file, or executable file.
You can use the alias instead of the command name in any Windows PowerShell commands. There are also implicit aliases: When PowerShell cannot find the cmdlet name, it will try to append `Get-` to the command as a last resort before, therefore e.g. `verb` will excute `Get-Verb`.

Every PowerShell author learns the actual command names, but different authors learn and use different aliases. Aliases can make code difficult to read, understand and
impact availability.

When developing PowerShell content that will potentially need to be maintained over time, either by the original author or others, you should use full command names.

The use of full command names also allows for syntax highlighting in sites and applications like GitHub and Visual Studio Code.

## How to Fix

Use the full cmdlet name and not an alias.

## Alias Whitelist

To prevent `PSScriptAnalyzer` from flagging your preferred aliases, create a whitelist of the aliases in your settings file and point `PSScriptAnalyzer` to use the settings file. For example, to disable `PSScriptAnalyzer` from flagging `cd`, which is an alias of `Set-Location`, set the settings file content to the following.

```PowerShell
# PSScriptAnalyzerSettings.psd1

@{
    'Rules' = @{
        'PSAvoidUsingCmdletAliases' = @{
            'Whitelist' = @('cd')
        }
    }
}
```

## Example

### Wrong?

``` PowerShell
gps | Where-Object {$_.WorkingSet -gt 20000000}
```

### Correct:

``` PowerShell
Get-Process | Where-Object {$_.WorkingSet -gt 20000000}
```
