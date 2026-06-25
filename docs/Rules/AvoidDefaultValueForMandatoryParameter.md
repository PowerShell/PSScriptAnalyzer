---
description: Avoid Default Value For Mandatory Parameter
ms.date: 06/12/2026
ms.topic: reference
title: AvoidDefaultValueForMandatoryParameter
---
# AvoidDefaultValueForMandatoryParameter

**Severity Level: Warning**

## Description

This rule detects when mandatory parameters have default values assigned. Mandatory parameters
shouldn't have default values because they can't be used. When a parameter is marked as mandatory,
PowerShell always prompts the user for a value if one isn't supplied when calling the function. Any
default value assigned to a mandatory parameter is unreachable and serves no purpose.

## Example

### Noncompliant

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

### Compliant

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
