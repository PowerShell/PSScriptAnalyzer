---
description: Use compatible cmdlets
ms.date: 07/21/2026
ms.topic: reference
title: UseCompatibleCmdlets
---
# UseCompatibleCmdlets

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects cmdlets that aren't available for the PowerShell edition, version, and operating
system you target. The rule compares each cmdlet in your script against a compatibility allow list
that ships with PSScriptAnalyzer. These files are in the following location:
`/path/to/PSScriptAnalyzerModule/Settings`.

Each file uses this naming format:

`<psedition>-<psversion>-<os>.json`

Where:

- `<psedition>` is `Core` or `Desktop`
- `<psversion>` is the PowerShell version
- `<os>` is `Windows`, `Linux`, `Linux-Arm`, or `MacOS`

## Configure rule

To check whether your script is compatible with PowerShell Core 6.1 on Windows, add this
configuration:

```powershell
@{
    'Rules' = @{
        'PSUseCompatibleCmdlets' = @{
            'compatibility'      = @('core-6.1.0-windows')
        }
    }
}
```

The **compatibility** parameter accepts a list of one or more target identifiers, such as:

- desktop-2.0-windows
- desktop-3.0-windows
- desktop-4.0-windows (taken from Windows Server 2012 R2)
- desktop-5.1.14393.206-windows
- core-6.1.0-windows (taken from Windows 10, version 1803)
- core-6.1.0-linux (taken from Ubuntu 18.04)
- core-6.1.0-linux-arm (taken from Raspbian)
- core-6.1.0-macos

Patched PowerShell versions usually have the same cmdlet data, so this rule mainly ships major and
minor version profiles.

You can also create a custom profile with [New-CommandDataFile.ps1][01]. Place the generated JSON
file in the `Settings` folder of the `PSScriptAnalyzer` module. Then you can reference that file by
name under `compatibility`.

The `core-6.0.2-*` files were removed in PSScriptAnalyzer 1.18 because PowerShell 6.0 reached its
end of life.

While the rule is configurable, the rule is always enabled. Use one of the following methods to
avoid using this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[01]: https://github.com/PowerShell/PSScriptAnalyzer/blob/main/Utils/New-CommandDataFile.ps1
[02]: ../using-scriptanalyzer.md
