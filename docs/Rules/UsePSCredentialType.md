---
description: Use PSCredential type
ms.date: 06/11/2026
ms.topic: reference
title: UsePSCredentialType
---
# UsePSCredentialType

**Severity Level: Warning**

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
