---
description: Use verbose message in DSC resources
ms.date: 07/21/2026
ms.topic: reference
title: DSCUseVerboseMessageInDSCResource
---
# UseVerboseMessageInDSCResource

**Severity Level: Information**

**Default state: Always enabled**

## Description

This rule detects DSC resources that don't include `Write-Verbose` messages in their functions or
scripts. It's a best practice to provide additional user information within commands, functions, and
scripts using [Write-Verbose][01]. This helps users understand what's happening during execution.
You should include `Write-Verbose` messages to make your code more informative and easier to debug.

## Example

### Noncompliant

```powershell
Function Test-Function
{
    [CmdletBinding()]
    Param()
    ...
}
```

### Compliant

```powershell
Function Test-Function
{
    [CmdletBinding()]
    Param()
    Write-Verbose 'Verbose output'
    ...
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- links reference -->
[01]: /powershell/module/microsoft.powershell.utility/write-verbose
[02]: ../using-scriptanalyzer.md
