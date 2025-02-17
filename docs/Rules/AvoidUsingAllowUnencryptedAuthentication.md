---
description: Avoid sending credentials and secrets over unencrypted connections
ms.date: 02/28/2024
ms.topic: reference
title: AvoidUsingAllowUnencryptedAuthentication
---
# AvoidUsingAllowUnencryptedAuthentication

**Severity Level: Warning**

## Description

Avoid using the **AllowUnencryptedAuthentication** parameter of `Invoke-WebRequest` and
`Invoke-RestMethod`. When using this parameter, the cmdlets send credentials and secrets over
unencrypted connections. This should be avoided except for compatibility with legacy systems.

For more details, see [Invoke-RestMethod](xref:Microsoft.PowerShell.Utility.Invoke-RestMethod).

## How

Avoid using the **AllowUnencryptedAuthentication** parameter.

## Example 1

### Wrong

```powershell
Invoke-WebRequest foo -AllowUnencryptedAuthentication
```

### Correct

```powershell
Invoke-WebRequest foo
```
