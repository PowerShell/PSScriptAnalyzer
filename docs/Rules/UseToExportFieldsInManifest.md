---
description: Use the *ToExport module manifest fields
ms.date: 07/21/2026
ms.topic: reference
title: UseToExportFieldsInManifest
---
# UseToExportFieldsInManifest

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
