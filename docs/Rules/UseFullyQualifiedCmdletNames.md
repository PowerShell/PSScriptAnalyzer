---
description: Use fully qualified module names when calling cmdlets and functions.
ms.date: 08/20/2025
ms.topic: reference
title: UseFullyQualifiedCmdletNames
---
# UseFullyQualifiedCmdletNames

**Severity Level: Warning**

## Description

PowerShell cmdlets and functions can be called with or without their module names. Using fully qualified names (with the module prefix) improves script clarity, reduces ambiguity, and helps ensure that the correct cmdlet is executed, especially in environments where multiple modules might contain cmdlets with the same name.

This rule identifies cmdlet and function calls that are not fully qualified and suggests adding the appropriate module qualifier.

# Configuration

```powershell
Rules = @{ 
    PSUseFullyQualifiedCmdletNames = @{ 
        Enable = $true 
        IgnoredModules = @() 
    } 
}
```


## How to Fix

Use the fully qualified cmdlet name in the format `ModuleName\CmdletName` instead of just `CmdletName`.

## Configuration Examples

### Enable the rule and enforce fully qualified names for all modules

```powershell
Rules = @{ 
    PSUseFullyQualifiedCmdletNames = @{ 
        Enable = $true
        IgnoredModules = @() 
    } 
}
```

### Enable the rule but ignore specific modules

```powershell
Rules = @{ 
    PSUseFullyQualifiedCmdletNames = @{ 
        Enable = $true
        IgnoredModules = @('Microsoft.PowerShell.Management', 'Microsoft.PowerShell.Utility') 
    } 
}
```

### Disable the rule

```powershell
Rules = @{ 
    PSUseFullyQualifiedCmdletNames = @{ 
        Enable = $false
    } 
}
```
	
### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation. By default, this rule is disabled and must be explicitly enabled.

#### IgnoredModules: string[] (Default value is `@()`)

Modules to ignore when applying this rule. Commands from these modules will not be flagged for expansion to their fully qualified names. By default, this is empty so all modules are checked.

## Examples

### Wrong

```powershell
# Unqualified cmdlet calls
Get-Command
Write-Host "Hello World"
Get-ChildItem -Path C:\temp

# Unqualified alias usage
gci C:\temp
ls -Force
```

### Correct
```powershell
# Fully qualified cmdlet calls
Microsoft.PowerShell.Core\Get-Command
Microsoft.PowerShell.Utility\Write-Host "Hello World"
Microsoft.PowerShell.Management\Get-ChildItem -Path C:\temp

# Fully qualified equivalents of aliases
Microsoft.PowerShell.Management\Get-ChildItem C:\temp
Microsoft.PowerShell.Management\Get-ChildItem -Force
```

## Benefits

- **Clarity**: Makes it explicit which module provides each cmdlet
- **Reliability**: Ensures the intended cmdlet is called, even if name conflicts exist
- **Module Auto-Loading**: Triggers PowerShell's module auto-loading mechanism, automatically importing the required module if it's not already loaded
- **Reduced Alias Conflicts**: Eliminates ambiguity that can arise from aliases that might conflict with cmdlets from different modules
- **Maintenance**: Easier to understand dependencies and troubleshoot issues
- **Best Practice**: Follows PowerShell best practices for production scripts
- **Performance**: Can improve performance by avoiding the need for PowerShell to search through multiple modules to resolve cmdlet names

## When to Use

This rule is particularly valuable for:

- Production scripts and modules
- Scripts shared across different environments
- Code that might run with varying module configurations
- Enterprise environments with custom or third-party modules

## Module Auto-Loading and Alias Considerations

### Auto-Loading Benefits

When you use fully qualified cmdlet names, PowerShell's module auto-loading feature provides several advantages:

- **Automatic Import**: If the specified module isn't already loaded, PowerShell will automatically import it when the cmdlet is called
- **Explicit Dependencies**: The script clearly declares which modules it depends on without requiring manual `Import-Module` calls
- **Version Control**: Helps ensure the correct module version is loaded, especially when multiple versions are installed

### Avoiding Alias Conflicts

Fully qualified names help prevent common issues with aliases:

- **Conflicting Aliases**: Different modules may define aliases with the same name but different behaviors
- **Platform Differences**: Some aliases behave differently across PowerShell versions or operating systems
- **Custom Aliases**: User-defined or organizational aliases won't interfere with script execution
- **Predictable Behavior**: Scripts behave consistently regardless of the user's alias configuration

### Example of Conflict Resolution

```powershell
# Without qualification - could resolve to different cmdlets depending on loaded modules
Get-Item

# With qualification - always resolves to the specific cmdlet
Microsoft.PowerShell.Management\Get-Item

# Alias that might conflict with custom definitions
ls

# Fully qualified equivalent that avoids conflicts
Microsoft.PowerShell.Management\Get-ChildItem
```

## Notes

- The rule analyzes cmdlets, functions, and aliases that can be resolved to a module
- Native commands (like `cmd.exe`) and user-defined functions without modules are not flagged
- Already qualified cmdlet calls are not flagged
- Variables and string literals containing cmdlet names are not flagged
- By default, all modules are checked; use `IgnoredModules` to exclude specific modules

## Related Rules

- [AvoidUsingCmdletAliases](./AvoidUsingCmdletAliases.md) - Recommends using full cmdlet names instead of aliases
- [UseCorrectCasing](./UseCorrectCasing.md) - Ensures correct casing for cmdlet names
