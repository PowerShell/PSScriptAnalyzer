---
description: Create hashtables with literal initializers
ms.date: 07/21/2026
ms.topic: reference
title: UseLiteralInitializerForHashtable
---
# UseLiteralInitializerForHashtable

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects hashtables created using the `[hashtable]::new()` method or the `New-Object
-TypeName hashtable` cmdlet without passing an `IEqualityComparer` object. When you create a
hashtable using `[hashtable]::new()` or `New-Object -TypeName hashtable`, the keys are looked up in
a case-sensitive manner by default.

However, PowerShell is case-insensitive in nature, and hashtables should maintain this behavior. To
ensure consistent case-insensitive key lookup, use literal hashtable expressions instead.

## Example

### Noncompliant

```powershell
$hashtable = [hashtable]::new()
```

### Compliant

```powershell
$hashtable = @{}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md