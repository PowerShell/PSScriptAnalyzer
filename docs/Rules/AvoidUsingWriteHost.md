---
description: Avoid using Write-Host cmdlet
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingWriteHost
---
# AvoidUsingWriteHost

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects usage of `Write-Host` in functions that don't use the `Show` verb. `Write-Host` is
designed to produce display-only output in the host, like printing colored text or prompting users
for input with `Read-Host`. It uses the `ToString()` method to write output, with results depending
on the PowerShell host program.

Since `Write-Host` doesn't send output to the pipeline, you'll need `Write-Output` or implicit
output to pass data down the pipeline.

Avoid using `Write-Host` in functions unless they use the `Show` verb, which explicitly means
_display information to the user_. This rule doesn't apply to functions with the `Show` verb.

Replace [Write-Host][01] with [Write-Output][02] or [Write-Verbose][03] based on your intention. Use
`Write-Verbose` for logging and `Write-Output` for returning objects.

## Example

### Noncompliant

```powershell
function Get-MeaningOfLife
{
    Write-Host 'Computing the answer to the ultimate question of life, the universe and everything'
    Write-Host 42
}
```

### Compliant

Use `Write-Verbose` for informational messages. The user can decide whether to see the message by
providing the **Verbose** parameter.

```powershell
function Get-MeaningOfLife {
    [CmdletBinding()]Param() # makes it possible to support Verbose output

    Write-Verbose 'Computing the answer to the ultimate question of life, the universe and everything'
    Write-Output 42
}

function Show-Something {
    Write-Host 'Show something on screen'
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][04].

<!-- links reference -->
[01]: /powershell/module/microsoft.powershell.utility/write-host
[02]: /powershell/module/microsoft.powershell.utility/write-output
[03]: /powershell/module/microsoft.powershell.utility/write-verbose
[04]: ../using-scriptanalyzer.md
