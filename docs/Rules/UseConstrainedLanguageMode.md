---
description: Use patterns compatible with Constrained Language Mode (CLM)
ms.date: 07/21/2026
ms.topic: reference
title: UseConstrainedLanguageMode
---
# UseConstrainedLanguageMode

**Severity Level: Warning**

**Default state: Disabled**

## Description

This rule detects PowerShell patterns that are restricted or not permitted in Constrained Language
Mode (CLM). Use it when scripts must remain compatible with restricted PowerShell environments such
as App Control for Business, AppLocker, or Just Enough Administration (JEA) endpoints. This rule is
**disabled** by default.

CLM is a PowerShell security feature that restricts:

- .NET types that can be used
- Component Object Model (COM) objects that can be instantiated
- Commands that can be executed
- Language features that can be used

CLM is commonly used in:

- Application Control environments (Application Control for Business, AppLocker)
- Just Enough Administration (JEA) endpoints
- Secure environments requiring other PowerShell restrictions

Digitally signed scripts from trusted publishers run in Full Language Mode (FLM), even in CLM
environments. The rule detects signature blocks (`# SIG # Begin signature block`) and adjusts its
checks accordingly. Most restrictions don't apply to signed scripts, but some checks are still
enforced.

> [!IMPORTANT]
> The rule performs a simple text check for signature blocks and doesn't validate signature
> authenticity or certificate trust. PowerShell performs actual signature validation at runtime.

## CLM restriction and remediation reference

Use the following table for a consolidated view of each CLM restriction, what it means, the
recommended remediation, and whether checks apply to signed scripts, unsigned scripts, or both.

|         Restriction          |                                                                                       What it means                                                                                        |                                                                  Recommended remediation                                                                   |                                             Enforced for                                              |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Add-Type                     | `Add-Type` compiles arbitrary C# code, which isn't permitted in CLM.                                                                                                                       | Use allowed cmdlets, or precompile, sign, and load assemblies.                                                                                             | Unsigned scripts only                                                                                 |
| Disallowed COM objects       | Only `Scripting.Dictionary`, `Scripting.FileSystemObject`, and `VBScript.RegExp` are allowed. Other COM objects (for example, `Excel.Application`) are flagged.                            | Use one of the allowed COM objects, or replace COM usage with PowerShell cmdlets.                                                                          | Unsigned scripts only                                                                                 |
| Disallowed .NET types        | Only about 70 .NET and type-accelerator types are allowed. These types include common primitives, selected collections, PowerShell-specific types, utilities, and arrays of allowed types. | Replace disallowed types with allowed accelerators such as `[string]`, `[int]`, and `[hashtable]`, or use cmdlets that avoid direct disallowed type usage. | Parameter type constraints are always enforced; other .NET type checks apply to unsigned scripts only |
| Type constraints             | Type constraints on parameters and variables are checked for CLM compatibility.                                                                                                            | Use allowed types in constraints, especially in parameter declarations.                                                                                    | Parameter constraints are always enforced; variable constraints are unsigned scripts only             |
| Type expressions             | Static references such as `[Type]::Method()` are flagged when they use disallowed types.                                                                                                   | Replace with CLM-compatible cmdlet-based patterns or allowed type usage.                                                                                   | Unsigned scripts only                                                                                 |
| Type casts                   | Casts to disallowed types are flagged.                                                                                                                                                     | Remove the cast or cast to an allowed type.                                                                                                                | Unsigned scripts only                                                                                 |
| Member invocations           | Method or property calls on disallowed typed objects are flagged.                                                                                                                          | Use allowed types and CLM-compatible cmdlets instead of disallowed typed objects.                                                                          | Unsigned scripts only                                                                                 |
| PowerShell classes           | The `class` keyword isn't permitted in CLM.                                                                                                                                                | Use `New-Object PSObject` with `Add-Member` or use hashtables. Avoid `[PSCustomObject]@{}` because it uses type casting.                                   | Unsigned scripts only                                                                                 |
| XAML or WPF                  | XAML or WPF usage isn't permitted in CLM.                                                                                                                                                  | Avoid XAML and WPF in CLM-compatible scripts.                                                                                                              | Unsigned scripts only                                                                                 |
| Invoke-Expression            | `Invoke-Expression` is restricted in CLM.                                                                                                                                                  | Use direct invocation (`&`) or safer alternatives.                                                                                                         | Unsigned scripts only                                                                                 |
| Dot-sourcing                 | Dot-sourcing can be restricted depending on the source location.                                                                                                                           | Prefer modules and `Import-Module` over dot-sourcing where possible.                                                                                       | All scripts (signed and unsigned)                                                                     |
| Module manifest wildcards    | Wildcards in `FunctionsToExport`, `CmdletsToExport`, `AliasesToExport`, or `VariablesToExport` aren't recommended and can be blocked in CLM contexts.                                      | Replace `*` with explicit export lists.                                                                                                                    | All `.psd1` files (signed and unsigned)                                                               |
| Module manifest `.ps1` files | Attempts to use `.ps1` for `RootModule`, `ModuleToProcess`, or `NestedModules` isn't CLM-friendly.                                                                                         | Use `.psm1` (script module) or `.dll` (binary module). Avoid `ScriptsToProcess`.                                                                           | All `.psd1` files (signed and unsigned)                                                               |

## Examples

### Add-Type

#### Noncompliant

```powershell
Add-Type -TypeDefinition @"
    public class Helper {
        public static string DoWork() { return "Done"; }
    }
"@
```

#### Compliant

```powershell
# Sign your scripts or modules by using appropriate signing tools
# (for example, Set-AuthenticodeSignature or an external signing process).
# Use allowed cmdlets instead of Add-Type-defined types where possible.
# Or precompile, sign, and load the assembly (for example, by using Add-Type -Path).
```

### COM objects

#### Noncompliant

```powershell
$excel = New-Object -ComObject Excel.Application
```

#### Compliant

```powershell
# Use allowed COM object
$dict = New-Object -ComObject Scripting.Dictionary

# Or use PowerShell cmdlets
Import-Excel -Path $file  # From ImportExcel module
```

### Disallowed types

#### Noncompliant

```powershell
# Type constraint and member invocation flagged
function Download-File {
    param([System.Net.WebClient]$Client)
    $Client.DownloadString($url)
}

# Type cast and method call flagged
[System.Net.WebClient]$client = New-Object System.Net.WebClient
$data = $client.DownloadData($url)
```

#### Compliant

```powershell
# Use allowed cmdlets
function Download-File {
    param([string]$Url)
    Invoke-WebRequest -Uri $Url
}

# Use allowed types
function Process-Text {
    param([string]$Text)
    $upper = $Text.ToUpper()  # String methods are allowed
}
```

### PowerShell classes

#### Noncompliant

```powershell
class MyClass {
    [string]$Name

    [string]GetInfo() {
        return $this.Name
    }
}

# Also wrong - uses type cast
$obj = [PSCustomObject]@{
    Name = "Test"
}
```

#### Compliant

```powershell
# Option 1: New-Object PSObject with Add-Member
$obj = New-Object PSObject -Property @{
    Name = "Test"
}

$obj | Add-Member -MemberType ScriptMethod -Name GetInfo -Value {
    return $this.Name
}

Add-Member -InputObject $obj -NotePropertyMembers @{"Number" = 42}

# Option 2: Hashtable
$obj = @{
    Name = "Test"
    Number = 42
}
```

### Module manifests

#### Noncompliant

```powershell
@{
    ModuleVersion = '1.0.0'
    RootModule = 'MyModule.ps1'        # .ps1 not recommended
    FunctionsToExport = '*'             # Wildcard not recommended
    CmdletsToExport = '*'
}
```

#### Compliant

```powershell
@{
    ModuleVersion = '1.0.0'
    RootModule = 'MyModule.psm1'       # Use .psm1 or .dll
    FunctionsToExport = @(              # Explicit list
        'Get-MyFunction'
        'Set-MyFunction'
    )
    CmdletsToExport = @()
}
```

### Array types

#### Noncompliant

```powershell
# Disallowed type in array
param([System.Net.WebClient[]]$Clients)
```

#### Compliant

```powershell
# Allowed types in arrays are fine
param([string[]]$Names)
param([int[]]$Numbers)
param([hashtable[]]$Configuration)
```

## Configure rule

```powershell
@{
    Rules = @{
        PSUseConstrainedLanguageMode = @{
            Enable = $true
        }
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks code against this rule. It accepts a boolean
value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### IgnoreSignatures

This parameter controls how the rule handles signature detection. It accepts a boolean value. The
default value is `$false`.

When this parameter is set to `$false`, ScriptAnalyzer automatically detects whether a script is
signed. Signed scripts get selective CLM checking, and unsigned scripts get full CLM checking.

When this parameter is set to `$true`, ScriptAnalyzer skips signature detection and applies full CLM
checking to all scripts, regardless of signature status.

The behavior is:

- `$false` (default): Automatically detect signatures. Signed scripts receive selective checking.
  Unsigned scripts receive full checks.
- `$true`: Bypass signature detection. All scripts get full CLM checking regardless of signature
  status.

```powershell
@{
    Rules = @{
        PSUseConstrainedLanguageMode = @{
            Enable = $true
            IgnoreSignatures = $true  # Enforce full CLM compliance for all scripts
        }
    }
}
```

Use `IgnoreSignatures = $true` when:

- Auditing signed scripts for complete CLM compatibility
- Preparing scripts for untrusted environments
- Enforcing strict CLM compliance organization-wide
- During development and testing, to see all potential issues

## See also

- [About Language Modes][01]
- [PowerShell Constrained Language Mode][03]
- [PowerShell Module Function Export in Constrained Language][04]
- [PowerShell Constrained Language Mode and the Dot-Source Operator][02]

<!-- link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_language_modes
[02]: https://devblogs.microsoft.com/powershell/powershell-constrained-language-mode-and-the-dot-source-operator/
[03]: https://devblogs.microsoft.com/powershell/powershell-constrained-language-mode/
[04]: https://devblogs.microsoft.com/powershell/powershell-module-function-export-in-constrained-language/
