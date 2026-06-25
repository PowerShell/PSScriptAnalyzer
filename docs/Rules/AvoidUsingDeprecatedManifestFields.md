---
description: Avoid using deprecated manifest fields
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingDeprecatedManifestFields
---
# AvoidUsingDeprecatedManifestFields

**Severity Level: Warning**

## Description

This rule detects the usage of deprecated manifest fields in module manifest files (`.psd1`) that
should be replaced with their modern equivalents. PowerShell 5.0 deprecated several fields in module
manifest files (`.psd1`) in favor of new alternatives.

The `ModuleToProcess` field, which was used to specify the module script file to load, is replaced
with the `RootModule` field. The `RootModule` field provides the same functionality and is the
recommended approach for new modules.

## Example

### Noncompliant

```powershell
ModuleToProcess ='psscriptanalyzer'

ModuleVersion = '1.0'
```

### Compliant

```powershell
RootModule ='psscriptanalyzer'

ModuleVersion = '1.0'
```
