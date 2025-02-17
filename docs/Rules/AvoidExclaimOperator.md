---
description: Avoid exclaim operator
ms.date: 03/26/2024
ms.topic: reference
title: AvoidExclaimOperator
---
# AvoidExclaimOperator

**Severity Level: Warning**

## Description

Avoid using the negation operator (`!`). Use `-not` for improved readability.

> [!NOTE]
> This rule is not enabled by default. The user needs to enable it through settings.

## How to Fix

## Example

### Wrong

```powershell
$MyVar = !$true
```

### Correct

```powershell
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

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.
