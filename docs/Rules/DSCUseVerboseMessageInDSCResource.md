---
description: Use verbose message in DSC resources
ms.date: 06/03/2026
ms.topic: reference
title: DSCUseVerboseMessageInDSCResource
---
# UseVerboseMessageInDSCResource

**Severity Level: Information**

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

<!-- links reference -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/write-verbose
