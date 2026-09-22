---
description: Avoid Using SecureString With Plain Text
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingConvertToSecureStringWithPlainText
---
# AvoidUsingConvertToSecureStringWithPlainText

**Severity Level: Error**

**Default state: Always enabled**

## Description

This rule detects the use of the `AsPlainText` parameter with the `ConvertTo-SecureString` command,
which bypasses encryption and exposes sensitive information in memory as plain text, defeating the
purpose of [SecureString][01].

Instead, retrieve secure credentials through encrypted channels or use secure input methods like
`Read-Host -AsSecureString` to ensure sensitive data remains encrypted throughout its lifecycle.

## Recommendations

If you need to retrieve passwords programmatically without user interaction, consider using the
[SecretStore][02] module from the PowerShell Gallery, which provides encrypted credential storage
and retrieval.

## Example

### Noncompliant

```powershell
$UserInput = Read-Host 'Please enter your secure code'
$EncryptedInput = ConvertTo-SecureString -String $UserInput -AsPlainText -Force
```

### Compliant

```powershell
$SecureUserInput = Read-Host 'Please enter your secure code' -AsSecureString
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][03].

<!-- Link references -->
[01]: /dotnet/api/system.security.securestring
[02]: https://www.powershellgallery.com/packages/Microsoft.PowerShell.SecretStore
[03]: ../using-scriptanalyzer.md
