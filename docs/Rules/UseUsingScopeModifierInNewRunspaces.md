---
description: Use 'Using:' scope modifier in RunSpace ScriptBlocks
ms.date: 06/28/2023
ms.topic: reference
title: UseUsingScopeModifierInNewRunspaces
---
# UseUsingScopeModifierInNewRunspaces

**Severity Level: Warning**

## Description

If a scriptblock is intended to be run in a new runspace, variables inside it should use the
`$using:` scope modifier, or be initialized within the scriptblock. This applies to:

- `Invoke-Command`- Only with the **ComputerName** or **Session** parameter.
- `Workflow { InlineScript {} }`
- `Foreach-Object` - Only with the **Parallel** parameter
- `Start-Job`
- `Start-ThreadJob`
- The `Script` resource in DSC configurations, specifically for the `GetScript`, `TestScript` and
  `SetScript` properties.

## How to Fix

Within the ScriptBlock, instead of just using a variable from the parent scope, you have to add the
`using:` scope modifier to it.

## Example

### Wrong

```powershell
$var = 'foo'
1..2 | ForEach-Object -Parallel { $var }
```

### Correct

```powershell
$var = 'foo'
1..2 | ForEach-Object -Parallel { $using:var }
```

## More correct examples

```powershell
$bar = 'bar'
Invoke-Command -ComputerName 'foo' -ScriptBlock { $using:bar }
```

```powershell
$bar = 'bar'
$s = New-PSSession -ComputerName 'foo'
Invoke-Command -Session $s -ScriptBlock { $using:bar }
```

```powershell
# Remark: Workflow is supported on Windows PowerShell only
Workflow {
    $foo = 'foo'
    InlineScript { $using:foo }
}
```

```powershell
$foo = 'foo'
Start-ThreadJob -ScriptBlock { $using:foo }
Start-Job -ScriptBlock {$using:foo }
```
