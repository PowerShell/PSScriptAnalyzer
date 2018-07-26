# AvoidGlobalAliases

**Severity Level: Warning**

## Description

Globally scoped aliases override existing aliases within the sessions with matching names. This name collision can cause difficult to debug issues for consumers of modules and scripts.

To understand more about scoping, see ```Get-Help about_Scopes```.

**NOTE** This rule is not available in PowerShell version 3 and 4 due to the `StaticParameterBinder.BindCommand` API that the rule uses internally.

## How

Use other scope modifiers for new aliases.

## Example

### Wrong

``` PowerShell
New-Alias -Name Name -Value Value -Scope "Global"
```

### Correct

``` PowerShell
New-Alias -Name Name1 -Value Value
```
