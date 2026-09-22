---
description: Module manifest fields
ms.date: 07/21/2026
ms.topic: reference
title: MissingModuleManifestField
---
# MissingModuleManifestField

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects when a module manifest is missing a required field. A module manifest is a `.psd1`
file that contains a hash table. The keys and values in the hash table describe the contents and
attributes of the module, define the prerequisites, and determine how the components are processed.

A module manifest must contain the following key-value pair to be considered valid:

- `ModuleVersion`

All other keys are optional and the order you place them doesn't matter. To learn more, see
[about_Module_Manifests][01].

## Example

### Noncompliant

```powershell
@{
    Author              = 'PowerShell Author'
    NestedModules       = @('.\mymodule.psm1')
    FunctionsToExport   = '*'
    CmdletsToExport     = '*'
    VariablesToExport   = '*'
}
```

### Compliant

```powershell
@{
    ModuleVersion       = '1.0'
    Author              = 'PowerShell Author'
    NestedModules       = @('.\mymodule.psm1')
    FunctionsToExport   = '*'
    CmdletsToExport     = '*'
    VariablesToExport   = '*'
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_module_manifests
[02]: ../using-scriptanalyzer.md
