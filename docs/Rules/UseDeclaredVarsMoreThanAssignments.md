---
description: Detect variables that are assigned but never used
ms.date: 06/10/2026
ms.topic: reference
title: UseDeclaredVarsMoreThanAssignments
---
# UseDeclaredVarsMoreThanAssignments

**Severity Level: Warning**

## Description

This rule detects variables that are assigned a value but never used. A variable counts as "used"
only when code references it in the same script block as its assignment. Variables that exist only
as assignments add noise to the code and should be removed.

## Example

### Noncompliant

```powershell
function Test
{
    $declaredVar = 'Declared just for fun'
    $declaredVar2 = 'Not used'
    Write-Output $declaredVar
}
```

### Compliant

```powershell
function Test
{
    $declaredVar = 'Declared just for fun'
    Write-Output $declaredVar
}
```

## Special cases

The following examples trigger the **PSUseDeclaredVarsMoreThanAssignments** warning. This behavior
is a limitation of the rule. There's no way to avoid these false positive warnings.

In this case, the warning is triggered because `$bar` isn't used within the scriptblock where it was
defined.

```powershell
$foo | ForEach-Object {
    if ($_ -eq $false) {
        $bar = $true
    }
}

if($bar){
    Write-Host 'Collection contained a false case.'
}
```

In the next example, the warning is triggered because `$errResult` isn't recognized as being used in
the `Write-Host` command.

```powershell
$errResult = $null
Write-Host 'Ugh:' -ErrorVariable errResult
```
