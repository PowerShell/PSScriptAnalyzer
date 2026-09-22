---
description: ReviewUnusedParameter
ms.date: 07/21/2026
ms.topic: reference
title: ReviewUnusedParameter
---
# ReviewUnusedParameter

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects parameters that are declared but not used in a script, scriptblock, or function
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

While this rule is configurable, it's always enabled. Use one of the following methods to avoid
using this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
