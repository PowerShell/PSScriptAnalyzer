---
description: Use a single ValueFromPipeline parameter per parameter set
ms.date: 06/25/2026
ms.topic: reference
title: UseSingleValueFromPipelineParameter
---
# UseSingleValueFromPipelineParameter

**Severity Level: Warning**

## Description

This rule detects functions where multiple parameters within the same parameter set are marked as
accepting pipeline input by value. Parameter sets should have at most one parameter with
`ValueFromPipeline = true`.

When you need multiple parameters to accept different types of pipeline input, use separate
parameter sets instead. Each parameter set can have its own single `ValueFromPipeline` parameter,
but you can't have more than one within the same parameter set.

## Example

### Noncompliant

```powershell
function Process-Data {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [string] $InputData,

        [Parameter(ValueFromPipeline)]
        [string] $ProcessingMode
    )

    process {
        Write-Output "$ProcessingMode`: $InputData"
    }
}
```

### Compliant

```powershell
function Process-Data {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [string] $InputData,

        [Parameter(Mandatory)]
        [string] $ProcessingMode
    )
    process {
        Write-Output "$ProcessingMode`: $InputData"
    }
}
```

## Suppression

This rule is disabled by default. If you have enabled it in your configuration but want to suppress
it for a specific function, you can use the `SuppressMessage` attribute:

```powershell
function Process-Data {
    [Diagnostics.CodeAnalysis.SuppressMessage('PSUseSingleValueFromPipelineParameter', 'MyParameterSet')]
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline, ParameterSetName='MyParameterSet')]
        [string] $InputData,

        [Parameter(ValueFromPipeline, ParameterSetName='MyParameterSet')]
        [string] $ProcessingMode
    )
    process {
        Write-Output "$ProcessingMode`: $InputData"
    }
}
```

For the default parameter set, use `'default'` as the suppression target:

```powershell
[Diagnostics.CodeAnalysis.SuppressMessage('PSUseSingleValueFromPipelineParameter', 'default')]
```

## Notes

- This rule applies to both explicit `ValueFromPipeline = $true` and implicit `ValueFromPipeline`
  (which is the same as using `= $true`).
- This rule doesn't flag parameters with `ValueFromPipeline = $false`.
- The rule correctly handles the default parameter set (`__AllParameterSets`) and named parameter
  sets.
- Different parameter sets can each have their own single `ValueFromPipeline` parameter without
  triggering this rule.
