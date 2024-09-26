---
description: ReviewUnusedParameter
ms.date: 03/26/2024
ms.topic: reference
title: ReviewUnusedParameter
---
# ReviewUnusedParameter

**Severity Level: Warning**

## Description

This rule identifies parameters declared in a script, scriptblock, or function scope that have not
been used in that scope.

## Configuration settings

By default, this rule doesn't consider child scopes other than scriptblocks provided to
`Where-Object` or `ForEach-Object`. The `CommandsToTraverse` setting is an string array allows you
to add additional commands that accept scriptblocks that this rule should examine.

```powershell
@{
    Rules = @{
        PSReviewUnusedParameter = @{
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
