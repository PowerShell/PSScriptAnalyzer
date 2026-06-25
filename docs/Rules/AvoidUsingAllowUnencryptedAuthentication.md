---
description: Avoid sending credentials and secrets over unencrypted connections
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingAllowUnencryptedAuthentication
---
# AvoidUsingAllowUnencryptedAuthentication

**Severity Level: Warning**

## Description

This rule detects the **AllowUnencryptedAuthentication** parameter used with `Invoke-WebRequest` and
`Invoke-RestMethod` cmdlets. When **AllowUnencryptedAuthentication** is used, it permits credentials
and secrets to be transmitted over unencrypted connections, creating a security risk.

Avoid using this parameter unless you must maintain compatibility with legacy systems that require
unencrypted authentication.

To learn more, see [Invoke-WebRequest][01] and [Invoke-RestMethod][02].

## Example

### Noncompliant

```powershell
Invoke-WebRequest foo -AllowUnencryptedAuthentication
```

### Compliant

```powershell
Invoke-WebRequest foo
```

<!-- links reference -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/invoke-webrequest
[02]: https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/invoke-restmethod
