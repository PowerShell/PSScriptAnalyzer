# AvoidUsingPositionalParameters

**Severity Level: Warning**

## Description

When developing PowerShell content that will potentially need to be maintained over time, either by the original author or others, you should use full command names and parameter names.

The use of positional parameters can reduce the readability of code and potentially introduce errors.

## How

Use full parameter names when calling commands.

## Example

### Wrong

``` PowerShell
Get-ChildItem *.txt
```

### Correct

``` PowerShell
Get-Content -Path *.txt
```
