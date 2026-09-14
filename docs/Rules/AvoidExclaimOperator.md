---
description: Avoid exclaim operator
ms.date: 07/21/2026
ms.topic: reference
title: AvoidExclaimOperator
---
# AvoidExclaimOperator

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule detects the use of the negation operator (an exclamation mark, `!`) and recommends using
the `-not` operator instead for improved readability and consistency with PowerShell conventions.

The `-not` operator is more explicit and aligns with PowerShell's verbose style, making code
easier to understand at a glance.

## Example

### Noncompliant

```powershell
$MyVar = !$true
```

### Compliant

```powershell
$MyVar = -not $true
```

## Configure rule

```powershell
Rules = @{
    PSAvoidExclaimOperator  = @{
        Enable = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.
