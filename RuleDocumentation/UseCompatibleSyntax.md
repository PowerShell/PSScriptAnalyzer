# UseCompatibleSyntax

**Severity Level: Warning**

## Description

This rule identifies syntax elements that are incompatible with targeted PowerShell versions.

It cannot identify syntax elements incompatible with PowerShell 3 or 4 when run from PowerShell 3 or
4 because those PowerShell versions aren't able to parse the incompatible syntaxes.

```powershell
@{
    Rules = @{
        PSUseCompatibleSyntax = @{
            Enable = $true
            TargetVersions = @(
                "6.0",
                "5.1",
                "4.0"
            )
        }
    }
}
```
