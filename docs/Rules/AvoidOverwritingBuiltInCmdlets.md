---
description: Avoid overwriting built-in cmdlets
ms.date: 06/01/2026
ms.topic: reference
title: AvoidOverwritingBuiltInCmdlets
---
# AvoidOverwritingBuiltInCmdlets

**Severity Level: Warning**

## Description

This rule detects and warns when a script defines a function that uses the name of a built-in cmdlet
available for a target PowerShell version and operating system. Overwriting built-in cmdlet names
can cause confusing behavior because callers might run your function when they expected the platform
cmdlet.

The rule compares function declarations in your script against command allow lists that ship with
PSScriptAnalyzer. The allow lists are also used by other compatibility rules. To learn more, see
[UseCompatibleCmdlets][01].

## Example

### Noncompliant

```powershell
function Get-ChildItem {
    param(
        [string]$Path = '.'
    )

    "Custom listing for: $Path"
}
```

### Compliant

```powershell
function Get-CustomChildItem {
    param(
        [string]$Path = '.'
    )

    "Custom listing for: $Path"
}
```

## Configure rule

To enable the rule to check if your script is compatible on PowerShell Core on Windows, add the
following lines in your settings file.

```powershell
@{
    'Rules' = @{
        'PSAvoidOverwritingBuiltInCmdlets' = @{
            'PowerShellVersion' = @('core-7.0.0-windows')
        }
    }
}
```

## Parameters

### PowerShellVersion

The `PowerShellVersion` parameter accepts one or more command allow list names that ship with
PSScriptAnalyzer. Set this value to the PowerShell version and platform you want to validate
against.

> [!NOTE]
> The default value for `PowerShellVersion` is `core-7.0.0-windows` if PowerShell 7 or later is
> installed, and `desktop-5.1.17763.316-windows` if it's not.

Patched PowerShell releases usually share the same cmdlet metadata, so the built-in allow lists are
provided by major and minor version. You can also generate a custom allow list with
[New-CommandDataFile.ps1][02]. To use a custom allow list, place the generated JSON file in the
`Settings` folder of the PSScriptAnalyzer module installation path, then set `PowerShellVersion` to
that filename.

The `core-6.0.2-*` files were removed in PSScriptAnalyzer 1.18 because PowerShell 6.0 reached end of
life.

<!-- link references -->

[01]: UseCompatibleCmdlets.md
[02]: https://github.com/PowerShell/PSScriptAnalyzer/blob/main/Utils/New-CommandDataFile.ps1
