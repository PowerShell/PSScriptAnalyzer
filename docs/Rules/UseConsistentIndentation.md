---
description: Use consistent indentation
ms.date: 06/09/2026
ms.topic: reference
title: UseConsistentIndentation
---
# UseConsistentIndentation

**Severity Level: Warning**

## Description

This rule detects inconsistent indentation patterns within a source file. Indentation should be
consistent throughout your script. This rule is **disabled** by default.

## Example

### Noncompliant

```powershell
function Get-ItemName {
    param(
      [string]$Path
    )

    if ($Path) {
      Get-Item $Path |
      Select-Object -ExpandProperty Name
    }
}
```

### Compliant

```powershell
function Get-ItemName {
    param(
        [string]$Path
    )

    if ($Path) {
        Get-Item $Path |
            Select-Object -ExpandProperty Name
    }
}
```

## Configure rule

```powershell
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
            PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
            Kind = 'space'
        }
    }
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### IndentationSize

This parameter controls the indentation size when `Kind` is set to `space`. It accepts an integer
value. To use four spaces per indentation level, set this parameter to `4`. The default value is
`4`.

### PipelineIndentation

This parameter controls pipeline indentation for multi-line statements. It accepts a string value.
The default value is `IncreaseIndentationForFirstPipeline`.

Acceptable values are:

- `IncreaseIndentationForFirstPipeline`: Indent once after the first pipeline and keep this
  indentation. Example:

  ```powershell
  foo |
      bar |
      baz
  ```

- `IncreaseIndentationAfterEveryPipeline`: Indent more after the first pipeline and keep this
  indentation. Example:

  ```powershell
  foo |
      bar |
          baz
  ```

- `NoIndentation`: Don't increase indentation. Example:

  ```powershell
  foo |
  bar |
  baz
  ```

- `None`: Don't change any existing pipeline indentation.

### Kind

This parameter controls which indentation character ScriptAnalyzer expects. It accepts a string
value. To use spaces for indentation, set this parameter to `space`. The default value is `space`.

When `Kind` is set to `space`, ScriptAnalyzer uses the value of `IndentationSize` for each
indentation level. When `Kind` is set to `tab`, ScriptAnalyzer uses a tab character (`\t`) per
indentation level. If you provide an invalid value, ScriptAnalyzer uses `space`.
