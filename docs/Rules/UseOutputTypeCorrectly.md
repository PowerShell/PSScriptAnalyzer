---
description: Use OutputType Correctly
ms.date: 06/28/2023
ms.topic: reference
title: UseOutputTypeCorrectly
---
# UseOutputTypeCorrectly

**Severity Level: Information**

## Description

A command should return the same type as declared in `OutputType`.

You can get more details by running `Get-Help about_Functions_OutputTypeAttribute` command in
PowerShell.

## How

Specify that the OutputType attribute lists and the types returned in the cmdlet match.

## Example

### Wrong

```powershell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )
        return 4
}
```

### Correct

```powershell
function Get-Foo
{
        [CmdletBinding()]
        [OutputType([String])]
        Param(
        )

        return 'four'
}
```
