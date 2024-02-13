---
description: Avoid sending credentials and secrets over unencrypted connections
ms.custom: PSSA v1.22.0
ms.date: 11/06/2022
ms.topic: reference
title: AvoidUsingAllowUnencryptedAuthentication
---
# AvoidUsingAllowUnencryptedAuthentication

**Severity Level: Warning**

## Description

Avoid using the `AllowUnencryptedAuthentication` switch on `Invoke-WebRequest`, `Invoke-RestMethod`, and other webrequest cmdlets, which sends credentials and secrets over unencrypted connections.
This should be avoided except for compatability with legacy systems.

For more details, see the documentation warning [here](https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/invoke-webrequest#-allowunencryptedauthentication).

## How

Avoid using the `AllowUnencryptedAuthentication` switch.

## Example 1

### Wrong

```powershell
Invoke-WebRequest foo -AllowUnencryptedAuthentication
```

### Correct

```powershell
Invoke-WebRequest foo
```