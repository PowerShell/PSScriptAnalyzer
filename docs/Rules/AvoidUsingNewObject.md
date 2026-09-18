---
description: Avoid using the `New-Object` cmdlet
ms.date: 06/06/2026
ms.topic: reference
title: AvoidUsingNewObject
---
# AvoidUsingNewObject

**Severity Level: Warning**

**Default state: Disabled**

## Description

Avoid using the `New-Object` cmdlet to create objects as it might perform poorly.
Instead, use a type initializer to construct or cast the intended object.

## Example

### Noncompliant

```powershell
# Create a version object using New-Object
$Version = New-Object -TypeName Version -ArgumentList "1.2.3"
```

### Compliant

```powershell
# Create a version object using the type constructor
$Version = [Version]"1.2.3"
```

## Examples

### Creating multiple Custom Objects

#### Noncompliant

```powershell
for ($i = 0; $i -lt 100000; $i++) {
    $resultObject = New-Object PSCustomObject -Property @{
        Name = "Name$i"
        Value = $i
    }
}
```

#### Compliant

```powershell
for ($i = 0; $i -lt 100000; $i++) {
    $resultObject = [PSCustomObject]@{
        Name = "Name$i"
        Value = $i
    }
}
```

### Creating a case-insensitive HashSet

#### Noncompliant

```powershell
$hashSet = New-Object -TypeName 'System.Collections.Generic.HashSet[String]' -ArgumentList ([StringComparer]::InvariantCultureIgnoreCase)
```

#### Compliant

```powershell
$hashSet = [System.Collections.Generic.HashSet[String]]::new([StringComparer]:::InvariantCultureIgnoreCase)
```

## Configure rule

This rule is disabled by default, but can be enabled by adding the following
configuration to your `PSScriptAnalyzerSettings.psd1` file:

```powershell
Rules = @{
    PSAvoidUsingNewObject  = @{
        Enable = $true
    }
}
```

## Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.
