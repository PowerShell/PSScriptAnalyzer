---
description: Use compatible syntax
ms.custom: PSSA v1.21.0
ms.date: 10/18/2021
ms.topic: reference
title: UseCompatibleSyntax
---
# UseCompatibleSyntax

**Severity Level: Warning**

## Description

This rule identifies syntax elements that are incompatible with targeted PowerShell versions.

It cannot identify syntax elements incompatible with PowerShell 3 or 4 when run from those
PowerShell versions because they aren't able to parse the incompatible syntaxes.

```powershell
@{
    Rules = @{
        PSUseCompatibleSyntax = @{
            Enable = $true
            TargetVersions = @(
                "6.0",
                "5.1",
                "4.0"
            )
        }
    }
}
```
