---
description: Avoid using username and password parameters
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingUsernameAndPasswordParams
---
# AvoidUsingUsernameAndPasswordParams

**Severity Level: Error**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

[02]: ../using-scriptanalyzer.md