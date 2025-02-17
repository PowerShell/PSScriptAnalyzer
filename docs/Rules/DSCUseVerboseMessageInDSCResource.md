---
description: Use verbose message in DSC resource
ms.date: 06/28/2023
ms.topic: reference
title: DSCUseVerboseMessageInDSCResource
---
# UseVerboseMessageInDSCResource

**Severity Level: Information**

## Description

Best practice recommends that additional user information is provided within commands, functions and
scripts using `Write-Verbose`.

## How

Make use of the `Write-Verbose` command.

## Example

### Wrong

```powershell
Function Test-Function
{
    [CmdletBinding()]
    Param()
    ...
}
```

### Correct

```powershell
Function Test-Function
{
    [CmdletBinding()]
    Param()
    Write-Verbose 'Verbose output'
    ...
}
```
