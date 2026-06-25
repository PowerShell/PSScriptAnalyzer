---
description: Use the *ToExport module manifest fields
ms.date: 06/11/2026
ms.topic: reference
title: UseToExportFieldsInManifest
---
# UseToExportFieldsInManifest

**Severity Level: Warning**

## Description

This rule detects when module manifests use wildcards (`'*'`) or null (`$null`) in export fields. To
improve module autodiscovery performance, module manifests shouldn't use wildcards or null in the
following entries:

- `AliasesToExport`
- `CmdletsToExport`
- `FunctionsToExport`
- `VariablesToExport`

When you use wildcards or null, PowerShell performs expensive analysis of your module during
autodiscovery. Instead, use an explicit list of items to export. If you have no items to export, use
an empty array (`@()`) instead of null or a wildcard.

## Example

### Noncompliant

```powershell
FunctionsToExport = $null
```

### Compliant

```powershell
FunctionsToExport = @()
```
