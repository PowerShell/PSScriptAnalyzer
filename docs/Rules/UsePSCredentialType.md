---
description: Use PSCredential type
ms.date: 07/21/2026
ms.topic: reference
title: UsePSCredentialType
---
# UsePSCredentialType

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects when a cmdlet or function defines a **Credential** parameter with a type other
than **PSCredential**. Credential parameters should always use the **PSCredential** type to ensure
proper handling of secure credential objects. Change any **Credential** parameter's type to
**PSCredential** for consistency and security.

## Example

### Noncompliant

```powershell
function Credential([String]$Credential)
{
    ...
}
```

### Compliant

```powershell
function Credential([PSCredential]$Credential)
{
    ...
}
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md
