---
description: Use process block for command that accepts input from pipeline.
ms.date: 06/28/2023
ms.topic: reference
title: UseProcessBlockForPipelineCommand
---
# UseProcessBlockForPipelineCommand

**Severity Level: Warning**

## Description

Functions that support pipeline input should always handle parameter input in a process block.
Unexpected behavior can result if input is handled directly in the body of a function where
parameters declare pipeline support.

## Example

### Wrong

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

#### Result

```
PS C:\> 1..5 | Get-Number
5
```

### Correct

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

#### Result

```
PS C:\> 1..5 | Get-Number
1
2
3
4
5
```
