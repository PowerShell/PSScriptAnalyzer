---
description: Extra Variables
ms.date: 03/06/2024
ms.topic: reference
title: UseDeclaredVarsMoreThanAssignments
---
# UseDeclaredVarsMoreThanAssignments

**Severity Level: Warning**

## Description

Variables that are assigned but not used are not needed.

> [!NOTE]
> For this rule, the variable must be used within the same scriptblock that it was declared or it
> won't be considered to be 'used'.

## How

Remove the variables that are declared but not used.

## Example

### Wrong

```powershell
function Test
{
    $declaredVar = 'Declared just for fun'
    $declaredVar2 = 'Not used'
    Write-Output $declaredVar
}
```

### Correct

```powershell
function Test
{
    $declaredVar = 'Declared just for fun'
    Write-Output $declaredVar
}
```

### Special cases

The following examples trigger the **PSUseDeclaredVarsMoreThanAssignments** warning. This behavior
is a limitation of the rule. There is no way to avoid these false positive warnings.

In this case, the warning is triggered because `$bar` is not used within the scriptblock where it
was defined.

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
