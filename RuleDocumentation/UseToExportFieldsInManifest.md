# UseToExportFieldsInManifest

**Severity Level: Warning**

## Description

To improve the performance of module auto-discovery, module manifests should not use wildcards (`'*'`) or null (`$null`) in the following entries:
* `AliasesToExport`
* `CmdletsToExport`
* `FunctionsToExport`
* `VariablesToExport`

The use of wildcards or null has the potential to cause PowerShell to perform expensive work to analyse a module during module auto-discovery.

## How

Use an explicit list in the entries.

## Example

Suppose there are no functions in your module to export. Then,

### Wrong

``` PowerShell
FunctionsToExport = $null
```

### Correct

``` PowerShell
FunctionToExport = @()
```

## Example

Suppose there are only two functions in your module, ```Get-Foo``` and ```Set-Foo``` that you want to export. Then,

### Wrong

``` PowerShell
FunctionsToExport = '*'
```

### Correct

``` PowerShell
FunctionToExport = @(Get-Foo, Set-Foo)
```
