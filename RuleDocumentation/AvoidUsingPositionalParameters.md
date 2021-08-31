# AvoidUsingPositionalParameters

** Severity Level: Information **

## Description

Using positional parameters reduces the readability of code and can introduce errors. It is possible
that a future version of the cmdlet could change in a way that would break existing scripts if calls
to the cmdlet rely on the position of the parameters.

For simple cmdlets with only a few positional parameters, the risk is much smaller. To prevent this
rule from being too noisy, this rule gets only triggered when there are 3 or more parameters
supplied. A simple example where the risk of using positional parameters is negligible, is
`Test-Path $Path`.

## How

Use full parameter names when calling commands.

## Example

### Wrong

```powershell
Get-Command ChildItem Microsoft.PowerShell.Management
```

### Correct

```powershell
Get-Command -Noun ChildItem -Module Microsoft.PowerShell.Management
```
