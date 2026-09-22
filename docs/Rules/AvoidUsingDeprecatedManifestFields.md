---
description: Avoid using deprecated manifest fields
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingDeprecatedManifestFields
---
# AvoidUsingDeprecatedManifestFields

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[02]: ../using-scriptanalyzer.md