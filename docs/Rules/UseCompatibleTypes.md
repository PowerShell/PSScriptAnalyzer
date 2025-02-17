---
description: Use compatible types
ms.date: 06/28/2023
ms.topic: reference
title: UseCompatibleTypes
---
# UseCompatibleTypes

**Severity Level: Warning**

## Description

This rule identifies types that are not available (loaded by default) in targeted PowerShell
platforms.

A PowerShell platform is identified by a name in the following format:

```
<os-name>_<os-arch>_<os-version>_<ps-version>_<ps-arch>_<dotnet-version>_<dotnet-edition>
```

Where:

- `<os-name>`: The name of the operating system PowerShell is running on.
    On Windows, this includes the SKU number.
    On Linux, this is the name of the distribution.
- `<os-arch>`: The machine architecture the operating system is running on (this is usually `x64`).
- `<os-version>`: The self-reported version of the operating system (on Linux, this is the
  distribution version).
- `<ps-version>`: The PowerShell version (from `$PSVersionTable.PSVersion`).
- `<ps-arch>`: The machine architecture of the PowerShell process.
- `<dotnet-version>`: The reported version of the .NET runtime PowerShell is running on (from
  `System.Environment.Version`).
- `<dotnet-edition>`: The .NET runtime flavor PowerShell is running on (currently `framework` or
  `core`).

For example:

- `win-4_x64_10.0.18312.0_5.1.18312.1000_x64_4.0.30319.42000_framework` is PowerShell 5.1 running on
  Windows 10 Enterprise (build 18312) for x64.
- `win-4_x64_10.0.18312.0_6.1.2_x64_4.0.30319.42000_core` is PowerShell 6.1.2 running on the same
  operating system.
- `ubuntu_x64_18.04_6.2.0_x64_4.0.30319.42000_core` is PowerShell 6.2.0 running on Ubuntu 18.04.

Some platforms come bundled with PSScriptAnalyzer as JSON files, named in this way for targeting in
your configuration.

Platforms bundled by default are:

| PowerShell Version |   Operating System    |                                  ID                                   |
| ------------------ | --------------------- | --------------------------------------------------------------------- |
| 3.0                | Windows Server 2012   | `win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework`              |
| 4.0                | Windows Server 2012R2 | `win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework`              |
| 5.1                | Windows Server 2016   | `win-8_x64_10.0.14393.0_5.1.14393.2791_x64_4.0.30319.42000_framework` |
| 5.1                | Windows Server 2019   | `win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework`  |
| 5.1                | Windows 10 1809 (RS5) | `win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework` |
| 6.2                | Windows Server 2016   | `win-8_x64_10.0.14393.0_6.2.4_x64_4.0.30319.42000_core`               |
| 6.2                | Windows Server 2019   | `win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core`               |
| 6.2                | Windows 10 1809 (RS5) | `win-4_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core`               |
| 6.2                | Ubuntu 18.04 LTS      | `ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core`                     |
| 7.0                | Windows Server 2016   | `win-8_x64_10.0.14393.0_7.0.0_x64_3.1.2_core`                         |
| 7.0                | Windows Server 2019   | `win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core`                         |
| 7.0                | Windows 10 1809 (RS5) | `win-4_x64_10.0.17763.0_6.2.4_x64_3.1.2_core`                         |
| 7.0                | Ubuntu 18.04 LTS      | `ubuntu_x64_18.04_6.2.4_x64_3.1.2_core`                               |

Other profiles can be found in the
[GitHub repo](https://github.com/PowerShell/PSScriptAnalyzer/tree/development/PSCompatibilityCollector/optional_profiles).

You can also generate your own platform profile using the
[PSCompatibilityCollector module](https://github.com/PowerShell/PSScriptAnalyzer/tree/development/PSCompatibilityCollector).

The compatibility profile settings takes a list of platforms to target under `TargetProfiles`. A
platform can be specified as:

- A platform name (like `ubuntu_x64_18.04_6.1.1_x64_4.0.30319.42000_core`), which will have `.json`
  added to the end and is searched for in the default profile directory.
- A filename (like `my_custom_platform.json`), which will be searched for the in the default profile
  directory.
- An absolute path to a file (like `D:\PowerShellProfiles\TargetMachine.json`).

The default profile directory is under the PSScriptAnalzyer module at
`$PSScriptRoot/PSCompatibilityCollector/profiles` (where `$PSScriptRoot` here refers to the
directory containing `PSScriptAnalyzer.psd1`).

The compatibility analysis compares a type used to both a target profile and a 'union' profile
(containing all types available in _any_ profile in the profile dir). If a type is not present in
the union profile, it is assumed to be locally created and ignored. Otherwise, if a type is present
in the union profile but not present in a target, it is deemed to be incompatible with that target.

## Configuration settings

| Configuration key |                                     Meaning                                      |                                     Accepted values                                     |                                   Mandatory                                   |                                                            Example                                                            |
| ----------------- | -------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| `Enable`          | Activates the rule                                                               | bool (`$true`/`$false`)                                                                 | No (default: `$false`)                                                        | `$true`                                                                                                                       |
| `TargetProfiles`  | The list of PowerShell profiles to target                                        | string[]: absolute paths to profile files or names of profiles in the profile directory | No (default: `@()`)                                                           | `@('ubuntu_x64_18.04_6.1.3_x64_4.0.30319.42000_core', 'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework')` |
| `ProfileDirPath`  | The location to search for profiles by name and use for union profile generation | string: absolute path to new profile dir                                                | No (defaults to `compatibility_profiles` directory in PSScriptAnalyzer module | `C:\Users\me\Documents\pssaCompatProfiles`                                                                                    |
| `IgnoreTypes`     | Full names of types or type accelerators to ignore compatibility of in scripts   | string[]: names of types to ignore                                                      | No (default: `@()`)                                                           | `@('System.Collections.ArrayList','string')`                                                                                  |

An example configuration might look like:

```powershell
@{
    Rules = @{
        PSUseCompatibleTypes = @{
            Enable = $true
            TargetProfiles = @(
                'ubuntu_x64_18.04_6.1.3_x64_4.0.30319.42000_core'
                'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
                'MyProfile'
                'another_custom_profile_in_the_profiles_directory.json'
                'D:\My Profiles\profile1.json'
            )
            # You can specify types to not check like this, which will also ignore methods and members on it:
            IgnoreTypes = @(
                'System.IO.Compression.ZipFile'
            )
        }
    }
}
```

Alternatively, you could provide a settings object as follows:

```powershell
PS> $settings = @{
      Rules = @{
        PSUseCompatibleTypes = @{
          Enable = $true
          TargetProfiles = @('win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework')
        }
      }
}
PS> Invoke-ScriptAnalyzer -Settings $settings -ScriptDefinition '[System.Management.Automation.SemanticVersion]'1.18.0-rc1''

RuleName                Severity     ScriptName Line  Message
--------                --------     ---------- ----  -------
PSUseCompatibleTypes    Warning                 1     The type 'System.Management.Automation.SemanticVersion' is
                                                      not available by default in PowerShell version
                                                      '5.1.17763.316' on platform 'Microsoft Windows 10 Pro'
```

## Suppression

Command compatibility diagnostics can be suppressed with an attribute on the `param` block of a
scriptblock as with other rules.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleTypes', '')]
```

The rule can also be suppressed only for particular types:

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleTypes', 'System.Management.Automation.Security.SystemPolicy')]
```

And also suppressed only for type members:

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleCommands', 'System.Management.Automation.LanguagePrimitives/ConvertTypeNameToPSTypeName')]
```
