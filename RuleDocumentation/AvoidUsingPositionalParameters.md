# AvoidUsingPositionalParameters

** Severity Level: Information **

## Description

When developing PowerShell content that will potentially need to be maintained over time, either by the original author or others, you should use full command names and parameter names.

The use of positional parameters can reduce the readability of code and potentially introduce errors. Furthermore it is possible that future signatures of a Cmdlet could change in a way that would break existing scripts if calls to the Cmdlet rely on the position of the parameters.

For simple Cmdlets with only a few positional parameters, the risk is much smaller and in order for this rule to be not too noisy, this rule gets only triggered when there are 3 or more parameters supplied. A simple example where the risk of using positional parameters is negligible, is e.g. `Test-Path $Path`.

## How

Use full parameter names when calling commands.

## Example

### Wrong

``` PowerShell
Get-Command Get-ChildItem Microsoft.PowerShell.Management System.Management.Automation.Cmdlet
```

### Correct

``` PowerShell
Get-Command -Noun Get-ChildItem -Module Microsoft.PowerShell.Management -ParameterType System.Management.Automation.Cmdlet
```
