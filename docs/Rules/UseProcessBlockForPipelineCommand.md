---
description: Use process block for commands that accept input from a pipeline
ms.date: 06/11/2026
ms.topic: reference
title: UseProcessBlockForPipelineCommand
---
# UseProcessBlockForPipelineCommand

**Severity Level: Warning**

## Description

This rule detects functions with pipeline-enabled parameters that handle input directly in the
function body instead of using a process block. Functions that accept pipeline input through
parameters should always process that input in a process block. Handling parameter input directly in
the function body can lead to unexpected behavior because the function body executes once, while the
process block executes for each object in the pipeline.

## Example

### Noncompliant

```powershell
Function Get-Number
{
    [CmdletBinding()]
    Param(
        [Parameter(ValueFromPipeline)]
        [int]
        $Number
    )

    $Number
}
```

### Compliant

```powershell
Function Get-Number
{
    [CmdletBinding()]
    Param(
        [Parameter(ValueFromPipeline)]
        [int]
        $Number
    )

    process
    {
        $Number
    }
}
```
