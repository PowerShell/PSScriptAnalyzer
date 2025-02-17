---
description: Avoid Default Value For Mandatory Parameter
ms.date: 06/28/2023
ms.topic: reference
title: AvoidDefaultValueForMandatoryParameter
---
# AvoidDefaultValueForMandatoryParameter

**Severity Level: Warning**

## Description

Mandatory parameters should not have a default values because there is no scenario where the default
can be used. PowerShell prompts for a value if the parameter value is not specified when calling the
function.

## Example

### Wrong

```powershell
function Test
{

    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        $Parameter1 = 'default Value'
    )
}
```

### Correct

```powershell
function Test
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true)]
        $Parameter1
    )
}
```
