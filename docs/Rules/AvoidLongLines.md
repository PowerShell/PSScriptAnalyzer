---
description: Avoid long lines
ms.date: 04/29/2025
ms.topic: reference
title: AvoidLongLines
---
# AvoidLongLines

**Severity Level: Warning**

## Description

The length of lines, including leading spaces (indentation), should less than the configured number
of characters. The default length is 120 characters.

> [!NOTE]
> This rule isn't enabled by default. The user needs to enable it through settings.

## Configuration

```powershell
Rules = @{
    PSAvoidLongLines  = @{
        Enable     = $true
        MaximumLineLength = 120
    }
}
```

## Parameters

### `Enable`: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

### `MaximumLineLength`: int (Default value is 120)

Optional parameter to override the default maximum line length.
