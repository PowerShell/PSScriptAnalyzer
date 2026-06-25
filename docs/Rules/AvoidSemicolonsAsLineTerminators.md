---
description: Avoid semicolons as line terminators
ms.date: 06/01/2026
ms.topic: reference
title: AvoidSemicolonsAsLineTerminators
---
# AvoidSemicolonsAsLineTerminators

**Severity Level: Warning**

## Description

This rule detects semicolons used as line terminators at the end of statements. In PowerShell,
line-ending semicolons are redundant and detract from code readability. Although semicolons serve as
statement separators on a single line, using them as line terminators is discouraged. Avoid using
semicolons at the end of lines.

This rule promotes cleaner, more maintainable code by removing unnecessary semicolons. This rule
isn't enabled by default.

## Example

### Noncompliant

```powershell
Install-Module -Name PSScriptAnalyzer; $a = 1 + $b;
```

```powershell
Install-Module -Name PSScriptAnalyzer;
$a = 1 + $b
```

### Compliant

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
    PSAvoidSemicolonsAsLineTerminators = @{
        Enable = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.
