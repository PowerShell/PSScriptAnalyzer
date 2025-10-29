---
description: Avoid Using New-Object
ms.date: 06/14/2025
ms.topic: reference
title: AvoidUsingNewObject
---
# AvoidUsingNewObject

**Severity Level: Warning**

## Description

The `New-Object` cmdlet should be avoided in modern PowerShell code except when creating COM objects. PowerShell provides more efficient, readable, and idiomatic alternatives for object creation that offer better performance and cleaner syntax.

This rule flags all uses of `New-Object` ***except*** when used with the `-ComObject` parameter, as COM object creation is one of the few legitimate remaining use cases for this cmdlet.

## Why Avoid New-Object?

### Performance Issues
`New-Object` uses reflection internally, which is significantly slower than direct type instantiation or accelerated type syntax. The performance difference becomes more pronounced in loops or frequently executed code.

### Readability and Maintainability
Modern PowerShell syntax is more concise and easier to read than `New-Object` constructions, especially for common .NET types.

### PowerShell Best Practices
The PowerShell community and Microsoft recommend using native PowerShell syntax over legacy cmdlets when better alternatives exist.

## Examples

### Wrong

```powershell
# Creating .NET objects
$list = New-Object System.Collections.Generic.List[string]
$hashtable = New-Object System.Collections.Hashtable
$stringBuilder = New-Object System.Text.StringBuilder
$datetime = New-Object System.DateTime(2023, 12, 25)

# Creating custom objects
$obj = New-Object PSObject -Property @{
    Name = "John"
    Age = 30
}
```

### Correct

```powershell
# Use accelerated type syntax for .NET objects
$list = [System.Collections.Generic.List[string]]::new()
$hashtable = @{}  # or [hashtable]@{}
$stringBuilder = [System.Text.StringBuilder]::new()
$datetime = [DateTime]::new(2023, 12, 25)

# Use PSCustomObject for custom objects
$obj = [PSCustomObject]@{
    Name = "John"
    Age = 30
}

# COM objects are still acceptable with New-Object
$excel = New-Object -ComObject Excel.Application
$word = New-Object -ComObject Word.Application
```

## Alternative Approaches

### For .NET Types
- **Static `new()` method**: `[TypeName]::new(parameters)`
- **Type accelerators**: Use built-in shortcuts like `@{}` for hashtables
- **Cast operators**: `[TypeName]$value` for type conversion

### For Custom Objects
- **PSCustomObject**: `[PSCustomObject]@{ Property = Value }`
- **Ordered dictionaries**: `[ordered]@{ Property = Value }`

### For Collections
- **Array subexpression**: `@(items)`
- **Hashtable literal**: `@{ Key = Value }`
- **Generic collections**: `[System.Collections.Generic.List[Type]]::new()`

## Performance Comparison

```powershell
# Slow - uses reflection
Measure-Command { 1..1000 | ForEach-Object { New-Object System.Text.StringBuilder } }

# Fast - direct instantiation
Measure-Command { 1..1000 | ForEach-Object { [System.Text.StringBuilder]::new() } }
```

The modern syntax provides a performance improvement over `New-Object` for most common scenarios.

## Exceptions

The rule allows `New-Object` when used with the `-ComObject` parameter because:
- COM object creation requires the `New-Object` cmdlet.
- No direct PowerShell alternative exists for COM instantiation.
- COM objects are external to the .NET type system.

```powershell
# This is acceptable
$shell = New-Object -ComObject WScript.Shell
$ie = New-Object -ComObject InternetExplorer.Application
```

---

## Migration Guide

| Old Syntax | New Syntax |
|------------|------------|
| **Creating a custom object**: <br> `New-Object PSObject -Property @{ Name = 'John'; Age = 30 }` | **Use PSCustomObject**: <br> `[PSCustomObject]@{ Name = 'John'; Age = 30 }` |
| **Creating a hashtable**: <br> `New-Object System.Collections.Hashtable` | **Use hashtable literal**: <br> `@{}` <br> **Or explicitly cast**: <br> `[hashtable]@{}` |
| **Creating a generic list**: <br> `New-Object 'System.Collections.Generic.List[string]'` | **Use static `new()` method**: <br> `[System.Collections.Generic.List[string]]::new()` |
| **Creating a DateTime object**: <br> `New-Object DateTime -ArgumentList 2023, 12, 25` | **Use static `new()` method**: <br> `[DateTime]::new(2023, 12, 25)` |
| **Creating a StringBuilder**: <br> `New-Object System.Text.StringBuilder` | **Use static `new()` method**: <br> `[System.Text.StringBuilder]::new()` |
| **Creating a process object**: <br> `New-Object System.Diagnostics.Process` | **Use static `new()` method**: <br> `[System.Diagnostics.Process]::new()` |
| **Creating a custom .NET object**: <br> `New-Object -TypeName 'Namespace.TypeName' -ArgumentList $args` | **Use static `new()` method**: <br> `[Namespace.TypeName]::new($args)` |

---

### Detailed Examples

#### Custom Object Creation

**Old Syntax:**

```powershell
$obj = New-Object PSObject -Property @{
    Name = 'John'
    Age = 30
}
```

**New Syntax:**

```powershell
$obj = [PSCustomObject]@{
    Name = 'John'
    Age = 30
}
```

#### Hashtable Creation

**Old Syntax:**

```powershell
$hashtable = New-Object System.Collections.Hashtable
$hashtable.Add('Key', 'Value')
```

**New Syntax:**

```powershell
$hashtable = @{
    Key = 'Value'
}
```

Or explicitly cast:

```powershell
$hashtable = [hashtable]@{
    Key = 'Value'
}
```

#### Generic List Creation

**Old Syntax:**

```powershell
$list = New-Object 'System.Collections.Generic.List[string]'
$list.Add('Item1')
$list.Add('Item2')
```

**New Syntax:**

```powershell
$list = [System.Collections.Generic.List[string]]::new()
$list.Add('Item1')
$list.Add('Item2')
```

#### DateTime Object Creation

**Old Syntax:**

```powershell
$date = New-Object DateTime -ArgumentList 2023, 12, 25
```

**New Syntax:**

```powershell
$date = [DateTime]::new(2023, 12, 25)
```

#### StringBuilder Creation

**Old Syntax:**

```powershell
$stringBuilder = New-Object System.Text.StringBuilder
$stringBuilder.Append('Hello')
```

**New Syntax:**

```powershell
$stringBuilder = [System.Text.StringBuilder]::new()
$stringBuilder.Append('Hello')
```

#### Custom .NET Object Creation

**Old Syntax:**

```powershell
$customObject = New-Object -TypeName 'Namespace.TypeName' -ArgumentList $arg1, $arg2
```

**New Syntax:**

```powershell
$customObject = [Namespace.TypeName]::new($arg1, $arg2)
```

#### Process Object Creation

**Old Syntax:**

```powershell
$process = New-Object System.Diagnostics.Process
```

**New Syntax:**

```powershell
$process = [System.Diagnostics.Process]::new()
```

---
## Related Links

- [New-Object][01]
- [PowerShell scripting performance considerations][02]
- [Creating .NET and COM objects][03]

<!-- link references -->
[01]: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/new-object
[02]: https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/performance/script-authoring-considerations
[03]: https://learn.microsoft.com/en-us/powershell/scripting/samples/creating-.net-and-com-objects--new-object-