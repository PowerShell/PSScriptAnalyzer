---
description: Module Manifest Fields
ms.date: 06/28/2023
ms.topic: reference
title: MissingModuleManifestField
---
# MissingModuleManifestField

**Severity Level: Warning**

## Description

A module manifest is a `.psd1` file that contains a hash table. The keys and values in the hash
table describe the contents and attributes of the module, define the prerequisites, and determine
how the components are processed.

Module manifests must contain the following keys (and a corresponding value) to be considered valid:

- `ModuleVersion`

All other keys are optional. The order of the entries is not important.

## How

Please consider adding the missing fields to the manifest.

## Example

### Wrong

```powershell
@{
    Author              = 'PowerShell Author'
    NestedModules       = @('.\mymodule.psm1')
    FunctionsToExport   = '*'
    CmdletsToExport     = '*'
    VariablesToExport   = '*'
}
```

### Correct

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
