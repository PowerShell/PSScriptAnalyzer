---
description: Cmdlet Singular Noun
ms.custom: PSSA v1.20.0
ms.date: 10/18/2021
ms.topic: reference
title: UseSingularNouns
---
# UseSingularNouns

**Severity Level: Warning**

## Description

PowerShell team best practices state cmdlets should use singular nouns and not plurals.

## How

Change plurals to singular.

## Example

### Wrong

```powershell
function Get-Files
{
    ...
}
```

### Correct

```powershell
function Get-File
{
    ...
}
```
