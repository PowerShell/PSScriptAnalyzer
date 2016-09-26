# UseCompatibleCmdlets
**Severity Level: Warning**

## Description
This rule flags cmdlets that are not available in a given Edition/Version of PowerShell on a given Operating System. It works by comparing a cmdlet against a set of whitelists which ship with PSScriptAnalyzer. They can be found at `/path/to/PSScriptAnalyzerModule/Settings`. These files are of the form, `PSEDITION-PSVERSION-OS.json` where `PSEDITION` can be either `core` or `desktop`, `OS` can be either `windows`, `linux` or `osx`, and `version` is the PowerShell version. To enable the rule to check if your script is compatible on PowerShell Core on windows, put the following your settings file:
```PowerShell
@{
    'Rules' = @{
        'PSUseCompatibleCmdlets' = @{
            'compatibility' = @("core-6.0.0-alpha-windows")
        }
    }
}
```

The parameter `compatibility` is a list that contain any of the following `{core-6.0.0-alpha-windows, core-6.0.0-alpha-linux, core-6.0.0-alpha-osx}`.