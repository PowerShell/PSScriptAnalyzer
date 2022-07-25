---
description: Extra Variables
ms.custom: PSSA v1.21.0
ms.date: 06/30/2022
ms.topic: reference
title: UseDeclaredVarsMoreThanAssignments
---
# UseDeclaredVarsMoreThanAssignments

**Severity Level: Warning**

## Description

Variables that are assigned but not used are not needed.

> [!NOTE]
> For this rule, the variable must be used within the same scriptblock that it was declared or it
> won't be considered to be "used".

## How

Remove the variables that are declared but not used.

## Example

### Wrong

```powershell
function Test
{
    $declaredVar = "Declared just for fun"
    $declaredVar2 = "Not used"
    Write-Output $declaredVar
}
```

### Correct

```powershell
function Test
{
    $declaredVar = "Declared just for fun"
    Write-Output $declaredVar
}
```

### Special case

The following example triggers the **PSUseDeclaredVarsMoreThanAssignments** warning because `$bar`
is not used within the scriptblock where it was defined.

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
