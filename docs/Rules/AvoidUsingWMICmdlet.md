---
description: Avoid using WMI cmdlets
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingWMICmdlet
---
# AvoidUsingWMICmdlet

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects the use of Windows Management Instrumentation (WMI) cmdlets. Since PowerShell 3.0,
you should use Common Information Model (CIM) cmdlets instead of WMI cmdlets. CIM cmdlets comply
with WS-Management (WSMan) standards and the CIM standard, which enables management of Windows and
non-Windows operating systems.

Don't use these WMI cmdlets:

- `Get-WmiObject`
- `Remove-WmiObject`
- `Invoke-WmiMethod`
- `Register-WmiEvent`
- `Set-WmiInstance`

Use these CIM cmdlets instead:

- `Get-CimInstance`
- `Remove-CimInstance`
- `Invoke-CimMethod`
- `Register-CimIndicationEvent`
- `Set-CimInstance`

## Example

### Noncompliant

```powershell
Get-WmiObject -Query 'Select * from Win32_Process where name LIKE "myprocess%"' | Remove-WmiObject
Invoke-WmiMethod -Class Win32_Process -Name 'Create' -ArgumentList @{ CommandLine = 'notepad.exe' }
```

### Compliant

```powershell
Get-CimInstance -Query 'Select * from Win32_Process where name LIKE "myprocess%"' | Remove-CimInstance
Invoke-CimMethod -ClassName Win32_Process -MethodName 'Create' -Arguments @{ CommandLine = 'notepad.exe' }
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

[02]: ../using-scriptanalyzer.md