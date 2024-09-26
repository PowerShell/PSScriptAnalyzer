---
description: Avoid semicolons as line terminators
ms.date: 06/28/2023
ms.topic: reference
title: AvoidSemicolonsAsLineTerminators
---
# AvoidSemicolonsAsLineTerminators

**Severity Level: Warning**

## Description

Lines should not end with a semicolon.

> [!NOTE]
> This rule is not enabled by default. The user needs to enable it through settings.

## Example

### Wrong

```powershell
Install-Module -Name PSScriptAnalyzer; $a = 1 + $b;
```

```powershell
Install-Module -Name PSScriptAnalyzer;
$a = 1 + $b
```

### Correct

```powershell
Install-Module -Name PSScriptAnalyzer; $a = 1 + $b
```

```powershell
Install-Module -Name PSScriptAnalyzer
$a = 1 + $b
```

## Configuration

```powershell
Rules = @{
    PSAvoidSemicolonsAsLineTerminators  = @{
        Enable     = $true
    }
}
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.
