---
description: Use at most a single ValueFromPipeline parameter per parameter set.
ms.date: 08/08/2025
ms.topic: reference
title: UseSingleValueFromPipelineParameter
---
# UseSingleValueFromPipelineParameter

**Severity Level: Warning**

## Description

Parameter sets should have at most one parameter marked as 
`ValueFromPipeline=true`.

This rule identifies functions where multiple parameters within the same
parameter set have `ValueFromPipeline` set to `true` (either explicitly or
implicitly).

## How

Ensure that only one parameter per parameter set accepts pipeline input by
value. If you need multiple parameters to accept different types of pipeline
input, use separate parameter sets.

## Example

### Wrong

```powershell
function Process-Data {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [string]$InputData,
        
        [Parameter(ValueFromPipeline)]
        [string]$ProcessingMode
    )
    
    process {
        Write-Output "$ProcessingMode`: $InputData"
    }
}
```


### Correct

```powershell
function Process-Data {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [string]$InputData,
        
        [Parameter(Mandatory)]
        [string]$ProcessingMode
    )
    process {
        Write-Output "$ProcessingMode`: $InputData"
    }
}
```
## Suppression

To suppress this rule for a specific parameter set, use the `SuppressMessage`
attribute with the parameter set name:

```powershell
function Process-Data {
    [Diagnostics.CodeAnalysis.SuppressMessage('UseSingleValueFromPipelineParameter', 'MyParameterSet')]
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline, ParameterSetName='MyParameterSet')]
        [string]$InputData,
        
        [Parameter(ValueFromPipeline, ParameterSetName='MyParameterSet')]
        [string]$ProcessingMode
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

- This rule applies to both explicit `ValueFromPipeline=$true` and implicit 
  `ValueFromPipeline` (which defaults to `$true`)
- Parameters with `ValueFromPipeline=$false` are not flagged by this rule
- The rule correctly handles the default parameter set (`__AllParameterSets`)
  and named parameter sets
- Different parameter sets can each have their own single `ValueFromPipeline`
  parameter without triggering this rule
