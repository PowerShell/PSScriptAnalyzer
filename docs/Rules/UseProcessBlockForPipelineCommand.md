---
description: Use process block for commands that accept input from a pipeline
ms.date: 07/21/2026
ms.topic: reference
title: UseProcessBlockForPipelineCommand
---
# UseProcessBlockForPipelineCommand

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
