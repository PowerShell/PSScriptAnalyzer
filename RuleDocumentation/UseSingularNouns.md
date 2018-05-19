# UseSingularNouns

**Severity Level: Warning**

## Description

PowerShell team best practices state cmdlets should use singular nouns and not plurals.

**NOTE** This rule is not available in PowerShell Core due to the PluralizationService API that the rule uses internally.

## How

Change plurals to singular.

## Example

### Wrong

``` PowerShell
function Get-Files
{
    ...
}
```

### Correct

``` PowerShell
function Get-File
{
    ...
}
```
