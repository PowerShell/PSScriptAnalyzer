---
description: Use a single ValueFromPipeline parameter per parameter set
ms.date: 07/21/2026
ms.topic: reference
title: UseSingleValueFromPipelineParameter
---
# UseSingleValueFromPipelineParameter

**Severity Level: Warning**

**Default state: Disabled**

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

## Configure rule

```powershell
Rules = @{
    PSUseSingleValueFromPipelineParameter  = @{
        Enable = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingleValueFromPipelineParameter', '')]
```

To suppress the rule for a single parameter set, pass the parameter set name as the second
argument. The name isn't case sensitive.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingleValueFromPipelineParameter', 'MyParameterSet')]
    param(
        [Parameter(ParameterSetName = 'MyParameterSet', ValueFromPipeline)]
        [String]$InputObject,

        [Parameter(ParameterSetName = 'MyParameterSet', ValueFromPipeline)]
        [String]$ProcessingMode
    )
}
```

For parameters that don't declare a parameter set name, use `default` as the second argument:

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingleValueFromPipelineParameter', 'default')]
```

## Notes

- This rule applies to both explicit `ValueFromPipeline = $true` and implicit `ValueFromPipeline`
  (which is the same as using `= $true`).
- This rule doesn't flag parameters with `ValueFromPipeline = $false`.
- The rule correctly handles the default parameter set (`__AllParameterSets`) and named parameter
  sets.
- Different parameter sets can each have their own single `ValueFromPipeline` parameter without
  triggering this rule.
