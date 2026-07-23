---
description: Avoid using cmdlet aliases or omitting the Get- prefix
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingCmdletAliases
---
# AvoidUsingCmdletAliases

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects the use of cmdlet aliases and omission of the `Get-` prefix in implicit alias
scenarios. An alias is an alternate name or nickname for a cmdlet or command element, such as a
function, script, file, or executable file. While you can use aliases instead of the full command
name, doing so reduces code readability and maintainability.

PowerShell also supports implicit aliases. When a cmdlet name isn't found, PowerShell appends
`Get-` to the command as a fallback. For example, typing `verb` executes `Get-Verb`.

Aliases create inconsistency across codebases because different authors learn and use different
aliases. Aliases can potentially make code difficult to read and understand in collaborative
environments. It can also cause issues with script portability and availability.

Using full cmdlet names improves code clarity, makes scripts easier to maintain, and enables proper
syntax highlighting in code editors and platforms like GitHub and Visual Studio Code. Always use the
full cmdlet name instead of aliases.

## Example

### Noncompliant

```powershell
gps | Where-Object {$_.WorkingSet -gt 20000000}
```

### Compliant

```powershell
Get-Process | Where-Object {$_.WorkingSet -gt 20000000}
```

## Configure rule

To prevent `PSScriptAnalyzer` from flagging your preferred aliases, create an allow list of the
aliases in your settings file and point `PSScriptAnalyzer` to use the settings file. For example, to
disable `PSScriptAnalyzer` from flagging `cd`, which is an alias of `Set-Location`, set the settings
file content to the following.

```powershell
@{
    'Rules' = @{
        'PSAvoidUsingCmdletAliases' = @{
            'allowlist' = @('cd')
        }
    }
}
```

While this rule is configurable, it's always enabled. Use one of the following methods to avoid
using this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[02]: ../using-scriptanalyzer.md