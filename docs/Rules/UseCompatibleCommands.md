---
description: Use compatible commands
ms.date: 06/09/2026
ms.topic: reference
title: UseCompatibleCommands
---
# UseCompatibleCommands

**Severity Level: Warning**

## Description

This rule detects commands that aren't available on your targeted PowerShell platform.

PowerShell platform names use the following format:

```
<os-name>_<os-arch>_<os-version>_<ps-version>_<ps-arch>_<dotnet-version>_<dotnet-edition>
```

Where:

- `<os-name>`: The name of the operating system PowerShell is running on. On Windows, the SKU number
  is included. On Linux, the value is the name of the distribution.
- `<os-arch>`: The machine architecture the operating system is running on (usually `x64`).
- `<os-version>`: The self-reported version of the operating system (the distribution version on
  Linux).
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

PSScriptAnalyzer includes some platform profiles as JSON files. You can target these built-in
profiles directly in your configuration.

Platforms bundled by default are:

| PowerShell Version |    Operating System    |                                  ID                                   |
| :----------------: | ---------------------- | --------------------------------------------------------------------- |
|        3.0         | Windows Server 2012    | `win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework`              |
|        4.0         | Windows Server 2012 R2 | `win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework`              |
|        5.1         | Windows Server 2016    | `win-8_x64_10.0.14393.0_5.1.14393.2791_x64_4.0.30319.42000_framework` |
|        5.1         | Windows Server 2019    | `win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework`  |
|        5.1         | Windows 10 Pro         | `win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework` |
|        6.2         | Ubuntu 18.04 LTS       | `ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core`                     |
|        6.2         | Windows 10.0.14393     | `win-8_x64_10.0.14393.0_6.2.4_x64_4.0.30319.42000_core`               |
|        6.2         | Windows 10.0.17763     | `win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core`               |
|        6.2         | Windows 10.0.18362     | `win-4_x64_10.0.18362.0_6.2.4_x64_4.0.30319.42000_core`               |
|        7.0         | Ubuntu 18.04 LTS       | `ubuntu_x64_18.04_7.0.0_x64_3.1.2_core`                               |
|        7.0         | Windows 10.0.14393     | `win-8_x64_10.0.14393.0_7.0.0_x64_3.1.2_core`                         |
|        7.0         | Windows 10.0.17763     | `win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core`                         |
|        7.0         | Windows 10.0.18362     | `win-4_x64_10.0.18362.0_7.0.0_x64_3.1.2_core`                         |

Other profiles can be found in the [GitHub repo][02].

You can also generate your own platform profile with the [PSCompatibilityCollector module][01].

Compatibility settings take a list of platforms under `TargetProfiles`. You can specify each target
platform as:

- A platform name (for example, `ubuntu_x64_18.04_6.1.1_x64_4.0.30319.42000_core`).
  PSScriptAnalyzer appends `.json` and searches for it in the default profile directory.
- A filename (for example, `my_custom_platform.json`), which PSScriptAnalyzer searches for in the
  default profile directory.
- An absolute path to a file (like `D:\PowerShellProfiles\TargetMachine.json`).

The default profile directory is under the PSScriptAnalyzer module at
`$PSScriptRoot/compatibility_profiles` (where `$PSScriptRoot` here refers to the directory
containing `PSScriptAnalyzer.psd1`).

The compatibility analysis compares each command you use against both a target profile and a union
profile. The union profile contains every command available in any profile in the profile
directory.

If a command isn't in the union profile, the rule assumes it's local to your environment and ignores
it. If a command is in the union profile but missing from a target profile, the rule flags it as
incompatible with that target.

## Example

The following examples assume `TargetProfiles` includes
`ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core` (Ubuntu 18.04, PowerShell 6.2).

### Noncompliant

```powershell
function Get-OsInfo {
    $os = Get-WmiObject -Class Win32_OperatingSystem
    return $os.Caption
}
```

### Compliant

```powershell
function Get-OsInfo {
    $os = Get-CimInstance -ClassName Win32_OperatingSystem
    return $os.Caption
}
```

## Configure rule

```powershell
@{
    Rules = @{
        PSUseCompatibleCommands = @{
            Enable = $true
            TargetProfiles = @(
                'ubuntu_x64_18.04_6.1.3_x64_4.0.30319.42000_core'
                'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
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

## Suppression

As with other rules, you can suppress command compatibility diagnostics by adding a suppression
attribute to the `param` block of a scriptblock.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleCommands', '')]
```

You can also suppress the rule for specific commands:

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleCommands',
    'Start-Service')]
```

You can also suppress it for specific parameters:

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseCompatibleCommands',
    'Import-Module/FullyQualifiedName')]
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### TargetProfiles

This parameter specifies the list of platform profiles to check compatibility against. It accepts
an array of strings. Each value can be a platform name, a filename, or an absolute path to a
profile file. The default value is `@()`.

### ProfileDirPath

This parameter controls the directory that ScriptAnalyzer searches for profiles by name and uses
to generate the union profile. It accepts a string containing an absolute path. The default
location is the `compatibility_profiles` directory in the PSScriptAnalyzer module.

### IgnoreCommands

This parameter specifies commands to exclude from compatibility checks. It accepts an array of
command-name strings. The default value is `@()`.

<!-- link references -->
[01]: https://github.com/PowerShell/PSScriptAnalyzer/tree/main/PSCompatibilityCollector
[02]: https://github.com/PowerShell/PSScriptAnalyzer/tree/main/PSCompatibilityCollector/optional_profiles
