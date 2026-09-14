---
description: Avoid sending credentials and secrets over unencrypted connections
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingAllowUnencryptedAuthentication
---
# AvoidUsingAllowUnencryptedAuthentication

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][03].

<!-- Link references -->
[01]: /powershell/module/microsoft.powershell.utility/invoke-webrequest
[02]: /powershell/module/microsoft.powershell.utility/invoke-restmethod
[03]: ../using-scriptanalyzer.md
