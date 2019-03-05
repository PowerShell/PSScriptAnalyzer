# UseCompatibleCmdlets

**Severity Level: Warning**

## Description

This rule flags cmdlets that are not available in a given Edition/Version of PowerShell on a given Operating System. It works by comparing a cmdlet against a set of whitelists which ship with PSScriptAnalyzer. They can be found at `/path/to/PSScriptAnalyzerModule/Settings`. These files are of the form, `PSEDITION-PSVERSION-OS.json` where `PSEDITION` can be either `Core` or `Desktop`, `OS` can be either `Windows`, `Linux` or `MacOS`, and `Version` is the PowerShell version. To enable the rule to check if your script is compatible on PowerShell Core on windows, put the following your settings file:

```PowerShell
@{
    'Rules' = @{
        'PSUseCompatibleCmdlets' = @{
            'compatibility' = @("core-6.1.0-windows")
        }
    }
}
```

The parameter `compatibility` is a list that contain any of the following

- desktop-2.0-windows
- desktop-3.0-windows
- desktop-4.0-windows (taken from Windows Server 2012R2)
- desktop-5.1.14393.206-windows
- core-6.1.0-windows (taken from Windows 10 - 1803)
- core-6.1.0-linux (taken from Ubuntu 18.04)
- core-6.1.0-linux-arm (taken from Raspbian)
- core-6.1.0-macos

Usually, patched versions of PowerShell have the same cmdlet data, therefore only settings of major and minor versions of PowerShell are supplied. One can also create a custom settings file as well with the [New-CommandDataFile.ps1](https://github.com/PowerShell/PSScriptAnalyzer/blob/development/Utils/New-CommandDataFile.ps1) script and use it by placing the created `JSON` into the `Settings` folder of the `PSScriptAnalyzer` module installation folder, then the `compatibility` parameter is just its file name (that can also be changed if desired).
Note that the `core-6.0.2-*` files were removed in PSScriptAnalyzer 1.18 since PowerShell 6.0 reached it's end of life.