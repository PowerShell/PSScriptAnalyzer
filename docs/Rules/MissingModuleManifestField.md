---
description: Module manifest fields
ms.date: 06/04/2026
ms.topic: reference
title: MissingModuleManifestField
---
# MissingModuleManifestField

**Severity Level: Warning**

## Description

This rule detects when a module manifest is missing required fields. A module manifest is a `.psd1`
file that contains a hash table. The keys and values in the hash table describe the contents and
attributes of the module, define the prerequisites, and determine how the components are processed.

A module manifest must contain the following key (and a corresponding value) to be considered valid:

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

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_module_manifests
