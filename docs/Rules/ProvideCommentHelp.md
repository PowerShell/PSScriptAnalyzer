---
description: Basic comment help
ms.date: 07/21/2026
ms.topic: reference
title: ProvideCommentHelp
---
# ProvideCommentHelp

**Severity Level: Information**

**Default state: Enabled**

## Description

This rule detects functions and cmdlets that don't have comment-based help. Every PowerShell command
should include comment-based help to document its purpose, parameters, and usage. PSScriptAnalyzer
checks for the presence of comment-based help but doesn't validate its content or format.

For assistance on comment-based help, use the command `Get-Help about_comment_based_help` or refer
to these resources:

- [Writing comment-based help][01]
- [Writing help for PowerShell cmdlets][02]
- [Create XML-based help using PlatyPS][03]

## Example

### Noncompliant

```powershell
function Get-File
{
    [CmdletBinding()]
    Param
    (
        ...
    )

}
```

### Compliant

```powershell
<#
.Synopsis
    Short description
.DESCRIPTION
    Long description
.EXAMPLE
    Example of how to use this cmdlet
.EXAMPLE
    Another example of how to use this cmdlet
.INPUTS
    Inputs to this cmdlet (if any)
.OUTPUTS
    Output from this cmdlet (if any)
.NOTES
    General notes
.COMPONENT
    The component this cmdlet belongs to
.ROLE
    The role this cmdlet belongs to
.FUNCTIONALITY
    The functionality that best describes this cmdlet
#>

function Get-File
{
    [CmdletBinding()]
    Param
    (
        ...
    )

}
```

## Configure rule

```powershell
Rules = @{
    PSProvideCommentHelp = @{
        Enable = $true
        ExportedOnly = $false
        BlockComment = $true
        VSCodeSnippetCorrection = $false
        Placement = 'before'
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. The default value is `$true`.

### ExportedOnly

This parameter controls whether violations are reported only for functions and cmdlets that are
exported using the `Export-ModuleMember` cmdlet. It accepts a boolean value. The default value
is `$true`.

### BlockComment

This parameter controls the style of comment help returned by the rule. It accepts a boolean
value. When set to `$true`, comment help is returned in block comment style (`<#...#>`).
When set to `$false`, comment help is returned in line comment style where each comment line
starts with `#`. The default value is `$true`.

### VSCodeSnippetCorrection

This parameter controls whether comment help is returned in Visual Studio Code snippet format. It
accepts a boolean value. The default value is `$false`.

### Placement

This parameter controls the position of comment help with respect to the function definition. It
accepts a string value. If any invalid value is given, the property defaults to `before`. The
default value is `before`.

The possible values are:

- `before`: The comment is placed before the function definition
- `begin`: The comment is placed at the beginning of the function definition body
- `end`: The comment is placed at the end of the function definition body

<!-- link references -->
[01]: /powershell/scripting/developer/help/writing-comment-based-help-topics
[02]: /powershell/scripting/developer/help/writing-help-for-windows-powershell-cmdlets
[03]: /powershell/utility-modules/platyps/create-help-using-platyps
