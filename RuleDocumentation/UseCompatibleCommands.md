# UseCompatibleCommands

**Severity Level: Warning**

## Description

This rule identifies commands that are not available on a targeted PowerShell platform.

A PowerShell platform is identified by a name in the following format:

```
<os-name>_<os-arch>_<os-version>_<ps-version>_<ps-arch>_<dotnet-version>_<dotnet-edition>
```

Where:

- `<os-name>`: The name of the operating system PowerShell is running on.
    On Windows, this includes the SKU number.
    On Linux, this is the name of the distribution.
- `<os-arch>`: The machine architecture the operating system is running on (this is usually `x64`).
- `<os-version>`: The self-reported version of the operating system (on Linux, this is the distribution version).
- `<ps-version>`: The PowerShell version (from `$PSVersionTable.PSVersion`).
- `<ps-arch>`: The machine architecture of the PowerShell process.
- `<dotnet-version>`: The reported version of the .NET runtime PowerShell is running on (from `System.Environment.Version`).
- `<dotnet-edition>`: The .NET runtime flavor PowerShell is running on (currently `framework` or `core`).

For example:

- `win-4_x64_10.0.18312.0_5.1.18312.1000_x64_4.0.30319.42000_framework` is PowerShell 5.1 running on Windows 10 Enterprise (build 18312) for x64.
- `win-4_x64_10.0.18312.0_6.1.2_x64_4.0.30319.42000_core` is PowerShell 6.1.2 running on the same operating system.
- `ubuntu_x64_18.04_6.2.0_x64_4.0.30319.42000_core` is PowerShell 6.2.0 running on Ubuntu 18.04.

Some platforms come bundled with PSScriptAnalyzer as JSON files, named in this way for targeting in your configuration.

Platforms bundled by default are:

| Platform                             | ID                                                                    |
| ------------------------------------ | --------------------------------------------------------------------- |
| Windows Server 2012 - PowerShell 3   | `win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework`              |
| Windows Server 2012R2 - PowerShell 4 | `win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework`              |
| Windows Server 2016 - PowerShell 5.1 | `win-8_x64_10.0.14393.0_5.1.14393.2636_x64_4.0.30319.42000_framework` |
| Windows Server 2019 - PowerShell 5.1 | `win-8_x64_10.0.17763.0_5.1.17763.134_x64_4.0.30319.42000_framework`  |
| Windows 10 - PowerShell 5.1          | `win-48_x64_10.0.17763.0_5.1.17763.134_x64_4.0.30319.42000_framework` |
| Windows Server 2019 - PowerShell 6.1 | `win-8_x64_10.0.14393.0_6.1.2_x64_4.0.30319.42000_core`               |
| Windows Server 2016 - PowerShell 6.1 | `win-8_x64_10.0.17763.0_6.1.2_x64_4.0.30319.42000_core`               |
| Windows 10 - PowerShell 6.1          | `win-48_x64_10.0.17763.0_6.1.2_x64_4.0.30319.42000_core`              |
| Ubuntu 18.04 LTS - PowerShell 6.1    | `ubuntu_x64_18.04_6.1.2_x64_4.0.30319.42000_core`                     |

Other profiles can be found [here](https://github.com/PowerShell/PSScriptAnalyzer/tree/development/CrossCompatibility/optional_profiles).

You can also generate your own platform profile using the [CrossCompatibility module](https://github.com/PowerShell/PSScriptAnalyzer/tree/development/CrossCompatibility).

The compatibility profile settings takes a list of platforms to target under `TargetProfiles`.
A platform can be specified as:

- A platform name (like `ubuntu_x64_18.04_6.1.1_x64_4.0.30319.42000_core`), which will have `.json` added to the end
  and is searched for in the default profile directory.
- A file name (like `my_custom_platform.json`), which will be searched for the in the default profile directory.
- An absolute path to a file (like `D:\PowerShellProfiles\TargetMachine.json`).

The default profile directory is under the PSScriptAnalzyer module at `$PSScriptRoot/CrossCompatibility/profiles`
(where `$PSScriptRoot` here refers to the directory containing `PSScriptAnalyzer.psd1`).

An example configuration might look like:

```PowerShell
@{
    Rules = @{
        UseCompatibleCommmands = @{
            Enabled = $true
            TargetProfiles = @(
                'ubuntu_x64_18.04_6.1.2_x64_4.0.30319.42000_core'
                'win-4_x64_10.0.18312.0_6.1.1_x64_4.0.30319.42000_framework'
                'MyProfile'
                'another_custom_profile_in_the_profiles_directory.json'
                'D:\My Profiles\profile1.json'
            )
            # You can specify commands to not check like this, which also will ignore its parameters:
            IgnoreCommands = @(
                'Install-Module'
            )
        }
    }
}
```