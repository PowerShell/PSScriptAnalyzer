---
description: ReviewUnusedParameter
ms.custom: PSSA v1.22.0
ms.date: 06/28/2023
ms.topic: reference
title: ReviewUnusedParameter
---
# ReviewUnusedParameter

**Severity Level: Warning**

## Description

This rule identifies parameters declared in a script, scriptblock, or function scope that have not
been used in that scope.

## Configuration settings

|Configuration key|Meaning|Accepted values|Mandatory|Example|
|---|---|---|---|---|
|CommandsToTraverse|By default, this command will not consider child scopes other than scriptblocks provided to Where-Object or ForEach-Object. This setting allows you to add additional commands that accept scriptblocks that this rule should traverse into.|string[]: list of commands whose scriptblock to traverse.|`@('Invoke-PSFProtectedCommand')`|

```powershell
@{
    Rules = @{
        ReviewUnusedParameter = @{
            CommandsToTraverse = @(
                'Invoke-PSFProtectedCommand'
            )
        }
    }
}
```

## How

Consider removing the unused parameter.

## Example

### Wrong

```powershell
function Test-Parameter
{
    Param (
        $Parameter1,

        # this parameter is never called in the function
        $Parameter2
    )

    Get-Something $Parameter1
}
```

### Correct

```powershell
function Test-Parameter
{
    Param (
        $Parameter1,

        # now this parameter is being called in the same scope
        $Parameter2
    )

    Get-Something $Parameter1 $Parameter2
}
```
