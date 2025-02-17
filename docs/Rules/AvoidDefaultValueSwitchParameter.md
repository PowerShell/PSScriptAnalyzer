---
description: Switch Parameters Should Not Default To True
ms.date: 06/28/2023
ms.topic: reference
title: AvoidDefaultValueSwitchParameter
---
# AvoidDefaultValueSwitchParameter

**Severity Level: Warning**

## Description

Switch parameters for commands should default to false.

## How

Change the default value of the switch parameter to be false.

## Example

### Wrong

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Param1,

        [switch]
        $Switch=$True
    )
    ...
}
```

### Correct

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Param1,

        [switch]
        $Switch=$False
    )
    ...
}
```
