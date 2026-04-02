---
description: Use the same pattern when defining parameters.
ms.date: 03/20/2026
ms.topic: reference
title: UseConsistentParametersKind
---
# UseConsistentParametersKind

**Severity Level: Warning**

## Description

All functions should use the same pattern when defining parameters. Possible pattern types are:

1. `Inline`

   ```powershell
   function f([Parameter()]$FirstParam) {
       return
   }
   ```

1. `ParamBlock`

   ```powershell
   function f {
       param([Parameter()]$FirstParam)
       return
   }
   ```

In simple scenarios, both function definitions shown are considered to be equal. The purpose of this
rule is to enforce consistent code style across the codebase.

## How to Fix

Rewrite function so it defines parameters as specified in the rule

## Example

When the rule sets parameters definition kind to `Inline`:

```powershell
# Correct
function f([Parameter()]$FirstParam) {
    return
}

# Incorrect
function g {
    param([Parameter()]$FirstParam)
    return
}
```

When the rule sets parameters definition kind to `ParamBlock`:

```powershell
# Incorrect
function f([Parameter()]$FirstParam) {
    return
}

# Correct
function g {
    param([Parameter()]$FirstParam)
    return
}
```