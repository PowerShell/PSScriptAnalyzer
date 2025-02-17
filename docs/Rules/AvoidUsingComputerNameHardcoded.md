---
description: Avoid Using ComputerName Hardcoded
ms.date: 06/28/2023
ms.topic: reference
title: AvoidUsingComputerNameHardcoded
---
# AvoidUsingComputerNameHardcoded

**Severity Level: Error**

## Description

The names of computers should never be hard coded as this will expose sensitive information. The
`ComputerName` parameter should never have a hard coded value.

## How

Remove hard coded computer names.

## Example 1

### Wrong

```powershell
Function Invoke-MyRemoteCommand ()
{
    Invoke-Command -Port 343 -ComputerName hardcoderemotehostname
}
```

### Correct

```powershell
Function Invoke-MyCommand ($ComputerName)
{
    Invoke-Command -Port 343 -ComputerName $ComputerName
}
```

## Example 2

### Wrong

```powershell
Function Invoke-MyLocalCommand ()
{
    Invoke-Command -Port 343 -ComputerName hardcodelocalhostname
}
```

### Correct

```powershell
Function Invoke-MyLocalCommand ()
{
    Invoke-Command -Port 343 -ComputerName $env:COMPUTERNAME
}
```
