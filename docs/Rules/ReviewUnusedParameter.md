---
description: ReviewUnusedParameter
ms.date: 06/08/2026
ms.topic: reference
title: ReviewUnusedParameter
---
# ReviewUnusedParameter

**Severity Level: Warning**

## Description

This rule detects parameters that are declared but not used a script, scriptblock, or function
scope. You should consider removing unused parameters to improve code clarity and reduce confusion
about your function's dependencies.

## Example

### Noncompliant

```powershell
function Test-Parameter
{
    Param (
        $Parameter1,

        # This parameter is never called in the function
        $Parameter2
    )

    Get-Something $Parameter1
}
```

### Compliant

```powershell
function Test-Parameter
{
    Param (
        $Parameter1,

        # Now this parameter is being called in the same scope
        $Parameter2
    )

    Get-Something $Parameter1 $Parameter2
}
```

## Configure rule

By default, this rule doesn't consider child scopes other than scriptblocks provided to either
`Where-Object` or `ForEach-Object`. The `CommandsToTraverse` setting is a string array that allows
you to add extra commands that accept scriptblocks that this rule should examine.

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
