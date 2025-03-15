---
description: Basic Comment Help
ms.date: 06/28/2023
ms.topic: reference
title: ProvideCommentHelp
---
# ProvideCommentHelp

**Severity Level: Info**

## Description

Comment based help should be provided for all PowerShell commands. This test only checks for the
presence of comment based help and not on the validity or format.

For assistance on comment based help, use the command `Get-Help about_comment_based_help` or the
following articles:

- [Writing Comment-based Help][01]
- [Writing Help for PowerShell Cmdlets][02]
- [Create XML-based help using PlatyPS][03]

## Configuration

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

### Parameters

- `Enable`: **bool** (Default value is `$true`)

  Enable or disable the rule during ScriptAnalyzer invocation.

- `ExportedOnly`: **bool** (Default value is `$true`)

  If enabled, throw violation only on functions/cmdlets that are exported using the
  `Export-ModuleMember` cmdlet.

- `BlockComment`: **bool** (Default value is `$true`)

  If enabled, returns comment help in block comment style (`<#...#>`). Otherwise returns
  comment help in line comment style where each comment line starts with `#`.

- `VSCodeSnippetCorrection`: **bool** (Default value is `$false`)

  If enabled, returns comment help in vscode snippet format.

- `Placement`: **string** (Default value is `before`)

  Represents the position of comment help with respect to the function definition.

  Possible values are:

  - `before`: means the help is placed before the function definition
  - `begin` means the help is placed at the beginning of the function definition body
  - `end` means the help is places the end of the function definition body

  If any invalid value is given, the property defaults to `before`.

## Example

### Wrong

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

### Correct

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
<!-- link references -->
[01]: https://learn.microsoft.com/powershell/scripting/developer/help/writing-comment-based-help-topics
[02]: https://learn.microsoft.com/powershell/scripting/developer/help/writing-help-for-windows-powershell-cmdlets
[03]: https://learn.microsoft.com/powershell/utility-modules/platyps/create-help-using-platyps
