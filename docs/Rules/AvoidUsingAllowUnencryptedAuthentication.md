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

## Suppression

As with other rules, you can suppress this rule by adding a suppression attribute to the `param`
block of a function or script. An empty string as the second argument suppresses every violation
of the rule in that scope.

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingAllowUnencryptedAuthentication', '')]
```

To suppress the rule for a single command, pass the command name as it appears in the call as the
second argument. The name isn't case sensitive, but it must match what's written in the script, so
a call made through an alias needs the alias name.

```powershell
function Test-Script {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingAllowUnencryptedAuthentication', 'Invoke-WebRequest')]
    param()

    Invoke-WebRequest foo -AllowUnencryptedAuthentication
}
```

<!-- Link references -->
[01]: /powershell/module/microsoft.powershell.utility/invoke-webrequest
[02]: /powershell/module/microsoft.powershell.utility/invoke-restmethod
[03]: ../using-scriptanalyzer.md#suppressing-rules
