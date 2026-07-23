---
description: Avoid long lines
ms.date: 07/21/2026
ms.topic: reference
title: AvoidLongLines
---
# AvoidLongLines

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule detects lines that exceed the configured maximum length, including leading spaces
(indentation). The default maximum line length is 120 characters.

## Example

### Noncompliant

```
In this example of sample text, the maximum line length is using the default setting of not exceeding more than 120
characters.
```

### Compliant

```
In this example of sample text, the maximum line length is set to not exceed
more than 80 characters.
```

## Configure rule

```powershell
Rules = @{
    PSAvoidLongLines = @{
        Enable = $true
        MaximumLineLength = 120
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### MaximumLineLength

This parameter is optional and defines the maximum length for a line before it violates the rule. It
accepts an integer value. The default value is `120`.
