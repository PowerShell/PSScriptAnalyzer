---
description: Use patterns compatible with Constrained Language Mode
ms.date: 03/20/2026
ms.topic: reference
title: UseConstrainedLanguageMode
---
# UseConstrainedLanguageMode

**Severity Level: Warning**

## Description

This rule identifies PowerShell patterns that are restricted or not permitted in Constrained
Language Mode (CLM).

Constrained Language Mode is a PowerShell security feature that restricts:

- .NET types that can be used
- COM objects that can be instantiated
- Commands that can be executed
- Language features that can be used

CLM is commonly used in:

- Application Control environments (Application Control for Business, AppLocker)
- Just Enough Administration (JEA) endpoints
- Secure environments requiring additional PowerShell restrictions

Digitally signed scripts from trusted publishers execute in Full Language Mode (FLM) even in CLM
environments. The rule detects signature blocks (`# SIG # Begin signature block`) and adjusts checks
accordingly. Most restrictions don't apply to signed scripts, but certain checks (dot-sourcing,
parameter types, manifest best practices) are always enforced.

> [!IMPORTANT]
> The rule performs a simple text check for signature blocks and does NOT validate signature
> authenticity or certificate trust. Actual signature validation is performed by PowerShell at
> runtime.

## Constrained Language Mode Restrictions

### Unsigned Scripts (Full CLM Checking)

The following are flagged for unsigned scripts:

1. **Add-Type** - Code compilation not permitted
1. **Disallowed COM Objects** - Only Scripting.Dictionary, Scripting.FileSystemObject,
   VBScript.RegExp allowed
1. **Disallowed .NET Types** - Only ~70 allowed types (string, int, hashtable, pscredential, etc.)
1. **Type Constraints** - On parameters and variables
1. **Type Expressions** - Static type references like `[Type]::Method()`
1. **Type Casts** - Converting to disallowed types
1. **Member Invocations** - Methods/properties on disallowed types
1. **PowerShell Classes** - `class` keyword not permitted
1. **XAML/WPF** - Not permitted
1. **Invoke-Expression** - Restricted
1. **Dot-Sourcing** - May be restricted depending on the file being sourced
1. **Module Manifest Wildcards** - Wildcard exports not recommended
1. **Module Manifest .ps1 Files** - Script modules ending with .ps1 not allowed

Always enforced, even for signed scripts

### Signed Scripts (Selective Checking)

For scripts with signature blocks, only these are checked:

- Dot-sourcing
- Parameter type constraints
- Module manifest wildcards (.psd1 files)
- Module manifest script modules (.psd1 files)

## Configuration

### Basic Configuration

```powershell
@{
    Rules = @{
        PSUseConstrainedLanguageMode = @{
            Enable = $true
        }
    }
}
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation. This rule is disabled by default
because not all scripts need CLM compatibility.

#### IgnoreSignatures: bool (Default value is `$false`)

Control signature detection behavior:

- `$false` (default): Automatically detect signatures. Signed scripts get selective checking,
  unsigned get full checking.
- `$true`: Bypass signature detection. ALL scripts get full CLM checking regardless of signature
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
- Development/testing to see all potential issues

## How to Fix

### Replace Add-Type

Use allowed cmdlets or pre-compile assemblies.

### Replace Disallowed COM Objects

Use only allowed COM objects (Scripting.Dictionary, Scripting.FileSystemObject, VBScript.RegExp) or
PowerShell cmdlets.

### Replace Disallowed Types

Use allowed type accelerators (`[string]`, `[int]`, `[hashtable]`, etc.) or allowed cmdlets instead
of disallowed .NET types.

### Replace PowerShell Classes

Use `New-Object PSObject` with `Add-Member` or hashtables instead of classes.

> [!IMPORTANT]
> `[PSCustomObject]@{}` syntax is NOT allowed in CLM because it uses type casting.

### Avoid XAML

Don't use WPF/XAML in CLM-compatible scripts.

### Replace Invoke-Expression

Use direct execution (`&`) or safer alternatives.

### Replace Dot-Sourcing

Use modules with Import-Module instead of dot-sourcing when possible.

### Fix Module Manifests

- Replace wildcard exports (`*`) with explicit lists.
- Use `.psm1` or `.dll` instead of `.ps1` for RootModule/NestedModules.
- Don't use `ScriptsToProcess`. These scripts are loaded in the caller's scope and are blocked.

## Examples

### Example 1: Add-Type

#### Wrong

```powershell
Add-Type -TypeDefinition @"
    public class Helper {
        public static string DoWork() { return "Done"; }
    }
"@
```

#### Correct

```powershell
 # Code sign your scripts/modules using proper signing tools
 #   (for example, Set-AuthenticodeSignature or external signing processes)
 # Use allowed cmdlets instead of Add-Type-defined types where possible
 # Or pre-compile, sign, and load the assembly (for example, via Add-Type -Path)
```

### Example 2: COM Objects

#### Wrong

```powershell
$excel = New-Object -ComObject Excel.Application
```

#### Correct

```powershell
# Use allowed COM object
$dict = New-Object -ComObject Scripting.Dictionary

# Or use PowerShell cmdlets
Import-Excel -Path $file  # From ImportExcel module
```

### Example 3: Disallowed Types

#### Wrong

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

#### Correct

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

### Example 4: PowerShell Classes

#### Wrong

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

#### Correct

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

### Example 5: Module Manifests

#### Wrong

```powershell
@{
    ModuleVersion = '1.0.0'
    RootModule = 'MyModule.ps1'        # .ps1 not recommended
    FunctionsToExport = '*'             # Wildcard not recommended
    CmdletsToExport = '*'
}
```

#### Correct

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

### Example 6: Array Types

#### Wrong

```powershell
# Disallowed type in array
param([System.Net.WebClient[]]$Clients)
```

#### Correct

```powershell
# Allowed types in arrays are fine
param([string[]]$Names)
param([int[]]$Numbers)
param([hashtable[]]$Configuration)
```

## Detailed Restrictions

### 1. Add-Type

`Add-Type` allows compiling arbitrary C# code and isn't permitted in CLM.

**Enforced For**: Unsigned scripts only

### 2. COM Objects

Only three COM objects are allowed:

- `Scripting.Dictionary`
- `Scripting.FileSystemObject`
- `VBScript.RegExp`

All others (Excel.Application, WScript.Shell, etc.) are flagged.

**Enforced For**: Unsigned scripts only

### 3. .NET Types

Only ~70 allowed types including:

- Primitives: `string`, `int`, `bool`, `byte`, `char`, `datetime`, `decimal`, `double`, etc.
- Collections: `hashtable`, `array`, `arraylist`
- PowerShell: `pscredential`, `psobject`, `securestring`
- Utilities: `regex`, `guid`, `version`, `uri`, `xml`
- Arrays: `string[]`, `int[][]`, etc. (array of any allowed type)

The rule checks type usage in:

- Parameter type constraints (**always enforced, even for signed scripts**)
- Variable type constraints
- New-Object -TypeName
- Type expressions (`[Type]::Method()`)
- Type casts (`[Type]$variable`)
- Member invocations on typed variables

**Enforced For**: Parameter constraints always; others unsigned only

### 4. PowerShell Classes

The `class` keyword is not permitted. Use `New-Object PSObject` with `Add-Member` or hashtables.

**Note**: `[PSCustomObject]@{}` is also not allowed because it uses type casting.

**Enforced For**: Unsigned scripts only

### 5. XAML/WPF

XAML and WPF are not permitted in CLM.

**Enforced For**: Unsigned scripts only

### 6. Invoke-Expression

`Invoke-Expression` is restricted in CLM.

**Enforced For**: Unsigned scripts only

### 7. Dot-Sourcing

Dot-sourcing (`. $PSScriptRoot\script.ps1`) may be restricted depending on source location.

**Enforced For**: ALL scripts (unsigned and signed)

### 8. Module Manifest Best Practices

#### Wildcard Exports

Don't use `*` in: `FunctionsToExport`, `CmdletsToExport`, `AliasesToExport`, `VariablesToExport`

Use explicit lists for security and clarity.

**Enforced For**: ALL .psd1 files (unsigned and signed)

#### Script Module Files

Don't use `.ps1` files in: `RootModule`, `ModuleToProcess`, `NestedModules`

Use `.psm1` (script modules) or `.dll` (binary modules) for better performance and compatibility.

**Enforced For**: ALL .psd1 files (unsigned and signed)

## More Information

- [About Language Modes][01]
- [PowerShell Constrained Language Mode][03]
- [PowerShell Module Function Export in Constrained Language][04]
- [PowerShell Constrained Language Mode and the Dot-Source Operator][02]

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_language_modes
[02]: https://devblogs.microsoft.com/powershell/powershell-constrained-language-mode-and-the-dot-source-operator/
[03]: https://devblogs.microsoft.com/powershell/powershell-constrained-language-mode/
[04]: https://devblogs.microsoft.com/powershell/powershell-module-function-export-in-constrained-language/
