---
description: Avoid using hardcoded computer names
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingComputerNameHardcoded
---
# AvoidUsingComputerNameHardcoded

**Severity Level: Error**

## Description

This rule detects hard-coded computer names in the `ComputerName` parameter of cmdlets. Hard-coded
computer names can expose sensitive information and reduce script portability. The `ComputerName`
parameter should always be parameterized or dynamically assigned to ensure scripts are flexible and
secure.

Use parameters, environment variables, or dynamic values instead of hard-coded computer
names.

## Example

### Noncompliant

```powershell
Function Invoke-MyRemoteCommand ()
{
    Invoke-Command -Port 343 -ComputerName HardcodedRemoteHostname
}
```

### Compliant

```powershell
Function Invoke-MyCommand ($ComputerName)
{
    Invoke-Command -Port 343 -ComputerName $ComputerName
}
```
