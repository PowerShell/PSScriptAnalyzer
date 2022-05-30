---
description: Use ShouldProcess For State Changing Functions
ms.custom: PSSA v1.21.0
ms.date: 10/18/2021
ms.topic: reference
title: UseShouldProcessForStateChangingFunctions
---
# UseShouldProcessForStateChangingFunctions

**Severity Level: Warning**

## Description

Functions whose verbs change system state should support `ShouldProcess`.

Verbs that should support `ShouldProcess`:

- `New`
- `Set`
- `Remove`
- `Start`
- `Stop`
- `Restart`
- `Reset`
- `Update`

## How

Include the `SupportsShouldProcess` argument in the `CmdletBinding` attribute.

## Example

### Wrong

```powershell
function Set-ServiceObject
{
    [CmdletBinding()]
    param
    (
        [string]
        $Parameter1
    )
    ...
}
```

### Correct

```powershell
function Set-ServiceObject
{
    [CmdletBinding(SupportsShouldProcess = $true)]
    param
    (
        [string]
        $Parameter1
    )
    ...
}
```
