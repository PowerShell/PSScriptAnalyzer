---
description: Avoid using the `New-Object` cmdlet
ms.date: 06/06/2026
ms.topic: reference
title: AvoidUsingNewObject
---
<!-- markdownlint-disable MD025 -->
# AvoidUsingNewObject
<!-- markdownlint-enable MD025 -->

<!-- markdownlint-disable MD036 -->
**Severity Level: Warning**
<!-- markdownlint-enable MD036 -->

## Description

Avoid using the `New-Object` cmdlet to create objects as it might perform poorly.
Instead, use a type initializer to construct or cast the intended object.

## Example

### Wrong

```powershell
# Create a version object using New-Object
$Version = New-Object -TypeName Version -ArgumentList "1.2.3"
```

```powershell
# Create a custom object using New-Object
for ($i = 0; $i -lt 100000; $i++) {
    $resultObject = New-Object PSCustomObject -Property @{
        Name = "Name$i"
        Value = $i
    }
}
```

```powershell
$hashSet = New-Object -TypeName 'System.Collections.Generic.HashSet[String]' -ArgumentList ([StringComparer]::InvariantCultureIgnoreCase)
```

### Correct

```powershell
# Create a version object using the type constructor
$Version = [Version]"1.2.3"
```

```powershell
# Create a custom object using a hashtable and type-casting
for ($i = 0; $i -lt 100000; $i++) {
    $resultObject = [PSCustomObject]@{
        Name = "Name$i"
        Value = $i
    }
}
```

```powershell
$hashSet = [System.Collections.Generic.HashSet[String]]::new([StringComparer]::InvariantCultureIgnoreCase)
```

## Configuration

```powershell
Rules = @{
    PSAvoidUsingNewObject  = @{
        Enable = $true
    }
}
```

### Parameters

- `Enable`: **bool** (Default value is `$false`)

  Enable or disable the rule during ScriptAnalyzer invocation.
