# AvoidUsingPositionalParameters

** Severity Level: Information **

## Description

When developing PowerShell content that will potentially need to be maintained over time, either by the original author or others, you should use full command names and parameter names.

The use of positional parameters can reduce the readability of code and potentially introduce errors.

For simple CmdLets with only a few parameters, the risk is much smaller and in order for this rule to be not too noisy, this rule gets only triggered when there are 3 or more parameters supplied.

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
