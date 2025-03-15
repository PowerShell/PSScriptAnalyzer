---
description: Avoid Using Write-Host
ms.date: 12/05/2024
ms.topic: reference
title: AvoidUsingWriteHost
---
# AvoidUsingWriteHost

**Severity Level: Warning**

## Description

The primary purpose of the `Write-Host` cmdlet is to produce display-only output in the host. For
example: printing colored text or prompting the user for input when combined with `Read-Host`.
`Write-Host` uses the `ToString()` method to write the output. The particular result depends on the
program that's hosting PowerShell. The output from `Write-Host` isn't sent to the pipeline. To
output data to the pipeline, use `Write-Output` or implicit output.

The use of `Write-Host` in a function is discouraged unless the function uses the `Show` verb. The
`Show` verb explicitly means _display information to the user_. This rule doesn't apply to functions
with the `Show` verb.

## How

Replace `Write-Host` with `Write-Output` or `Write-Verbose` depending on whether the intention is
logging or returning one or more objects.

## Example

### Wrong

```powershell
function Get-MeaningOfLife
{
    Write-Host 'Computing the answer to the ultimate question of life, the universe and everything'
    Write-Host 42
}
```

### Correct

Use `Write-Verbose` for informational messages. The user can decide whether to see the message by
providing the **Verbose** parameter.

```powershell
function Get-MeaningOfLife
{
    [CmdletBinding()]Param() # makes it possible to support Verbose output

    Write-Verbose 'Computing the answer to the ultimate question of life, the universe and everything'
    Write-Output 42
}

function Show-Something
{
    Write-Host 'show something on screen'
}
```

## More information

[Write-Host](xref:Microsoft.PowerShell.Utility.Write-Host)
