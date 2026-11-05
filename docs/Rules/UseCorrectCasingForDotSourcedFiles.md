---
description: Use the exact on-disk casing of a dot-sourced file's path
ms.date: 09/23/2026
ms.topic: reference
title: UseCorrectCasingForDotSourcedFiles
---
# UseCorrectCasingForDotSourcedFiles

**Severity Level: Warning**

**Default state: Disabled**

## Description

Dot-sourcing (`. .\path\to\file.ps1`) resolves the target path through the operating system's file
APIs. On a case-insensitive filesystem (the Windows default, and macOS's default APFS format), a
path whose casing does not exactly match the file on disk still resolves, silently. On a
case-sensitive filesystem (most Linux filesystems, or an explicitly case-sensitive APFS or NTFS
volume), the same script fails at run time with an "item not found" error, even though
PSScriptAnalyzer's static parse of the referenced file succeeds.

This rule flags a dot-sourced path whose file name does not match the exact casing of the file it
resolves to, so the mismatch is caught during analysis rather than only on a platform, or volume,
where casing happens to matter.

To follow this rule, match the exact, on-disk casing of the dot-sourced file's name.

## Example

### Noncompliant

The file on disk is actually named `Helpers.ps1`.

```powershell
. $PSScriptRoot\HELPERS.ps1
```

### Compliant

```powershell
. $PSScriptRoot\Helpers.ps1
```

## Configure rule

```powershell
Rules = @{
    PSUseCorrectCasingForDotSourcedFiles = @{
        Enable = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks code against this rule. It accepts a boolean
value. To enable this rule, set this parameter to `$true`. The default value is `$false`.
