---
description: Avoid exclaim operator
ms.custom: PSSA v1.21.0
ms.date: 06/14/2023
ms.topic: reference
title: AvoidExclaimOperator
---
# AvoidExclaimOperator
**Severity Level: Warning**

## Description

The negation operator `!` should not be used for readability purposes. Use `-not` instead.

**Note**: This rule is not enabled by default. The user needs to enable it through settings.

## How to Fix

## Example
### Wrong：
```PowerShell
$MyVar = !$true
```

### Correct:
```PowerShell
$MyVar = -not $true
```

## Configuration

```powershell
Rules = @{
    PSAvoidExclaimOperator  = @{
        Enable = $true
    }
}
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.