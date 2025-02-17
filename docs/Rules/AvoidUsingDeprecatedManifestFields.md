---
description: Avoid Using Deprecated Manifest Fields
ms.date: 06/28/2023
ms.topic: reference
title: AvoidUsingDeprecatedManifestFields
---
# AvoidUsingDeprecatedManifestFields

**Severity Level: Warning**

## Description

In PowerShell 5.0, a number of fields in module manifest files (`.psd1`) have been changed.

The field `ModuleToProcess` has been replaced with the `RootModule` field.

## How

Replace `ModuleToProcess` with `RootModule` in the module manifest.

## Example

### Wrong

```powershell
ModuleToProcess ='psscriptanalyzer'

ModuleVersion = '1.0'
```

### Correct

```powershell
RootModule ='psscriptanalyzer'

ModuleVersion = '1.0'
```
