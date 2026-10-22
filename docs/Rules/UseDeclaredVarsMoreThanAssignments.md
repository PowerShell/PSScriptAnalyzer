---
description: Detect variables that are assigned but never used
ms.date: 07/21/2026
ms.topic: reference
title: UseDeclaredVarsMoreThanAssignments
---
# UseDeclaredVarsMoreThanAssignments

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][01].

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignments', '')]
```

To suppress the rule for a single variable, pass the variable name without the leading `$` as the
second argument. The name isn't case sensitive. This is useful for the false positives described
in the Special cases section.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignments', 'UnusedVariable')]
    param()

    $UsedVariable = 'Used'
    $UnusedVariable = 'Unused'
    Write-Output $UsedVariable
}
```

<!-- link references -->
[01]: ../using-scriptanalyzer.md#suppressing-rules
