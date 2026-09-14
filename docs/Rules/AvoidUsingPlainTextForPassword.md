---
description: Avoid using plain text for Password parameter
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingPlainTextForPassword
---
# AvoidUsingPlainTextForPassword

**Default state: Always enabled**

**Severity Level: Warning**

## Description

This rule detects function or script parameters with password-related names that are declared with a
`string` type instead of the `SecureString` type. Password parameters that accept plaintext expose
passwords and can compromise your system's security. You shouldn't accept passwords as plain text.
Instead, use the [SecureString][01] type to store passwords securely.

The following parameters are considered password parameters and aren't case sensitive:

- Password
- Pass
- Passwords
- Passphrase
- Passphrases
- PasswordParam

If you define a parameter with a name from the provided list, declare it with the **SecureString**
type.

## Example

### Noncompliant

```powershell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [string]
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
        [SecureString]
        $Password
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

<!-- link references -->
[01]: /dotnet/api/system.security.securestring
[02]: ../using-scriptanalyzer.md
