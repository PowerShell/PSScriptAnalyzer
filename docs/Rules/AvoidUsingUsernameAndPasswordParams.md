---
description: Avoid using username and password parameters
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingUsernameAndPasswordParams
---
# AvoidUsingUsernameAndPasswordParams

**Severity Level: Error**

## Description

This rule detects functions that use separate username and password parameters instead of a single
**PSCredential** object. Functions shouldn't use separate username or password parameters. To
standardize command parameters, credentials should be accepted as objects of type **PSCredential**.

## Example

### Noncompliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Username,
        [SecureString]
        $Password
    )
    ...
}
```

### Compliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [PSCredential]
        $Credential
    )
    ...
}
```
