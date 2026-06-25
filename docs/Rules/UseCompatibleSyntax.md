---
description: Use compatible syntax
ms.date: 06/09/2026
ms.topic: reference
title: UseCompatibleSyntax
---
# UseCompatibleSyntax

**Severity Level: Warning**

## Description

This rule detects syntax elements that aren't compatible with your specified PowerShell target
versions. When you run this rule from PowerShell 3 or 4, it can't detect syntax that's incompatible
with those versions because those versions can't parse newer syntax constructs.

## Example

### Noncompliant

The following example uses PowerShell 7 syntax. If `TargetVersions` includes `5.1` or earlier, this
rule flags these statements.

```powershell
$message = $IsReady ? 'Ready' : 'Not ready'
$displayName = $name ?? 'Unknown'
```

### Compliant

```powershell
if ($IsReady) {
    $message = 'Ready'
} else {
    $message = 'Not ready'
}

if ($name -ne $null) {
    $displayName = $name
} else {
    $displayName = 'Unknown'
}
```

## Configure rule

```powershell
@{
    Rules = @{
        PSUseCompatibleSyntax = @{
            Enable = $true
            TargetVersions = @(
                '6.0',
                '5.1',
                '4.0'
            )
        }
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### TargetVersions

This parameter specifies the PowerShell versions your code must be compatible with. It accepts an
array of version strings, such as `'7.0'`, `'5.1'`, or `'4.0'`. ScriptAnalyzer flags syntax that's
not supported in any version you target.
