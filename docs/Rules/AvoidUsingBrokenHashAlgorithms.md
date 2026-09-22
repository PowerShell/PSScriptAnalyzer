---
description: Avoid using broken hash algorithms
ms.date: 07/21/2026
ms.topic: reference
title: AvoidUsingBrokenHashAlgorithms
---
# AvoidUsingBrokenHashAlgorithms

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects the use of cryptographically broken hash algorithms `MD5` and `SHA-1`. Avoid using
hash algorithms `MD5` and `SHA-1`. These algorithms are vulnerable to collision attacks and are no
longer considered secure for cryptographic purposes.

Replace `MD5` and `SHA-1` with secure alternatives such as `SHA256`, `SHA384`, or `SHA512`. Use
broken algorithms only when strictly necessary for backwards compatibility with legacy systems.

## Example

### Noncompliant

```powershell
Get-FileHash foo.txt -Algorithm MD5
```

### Compliant

```powershell
Get-FileHash foo.txt -Algorithm SHA256
```

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[02]: ../using-scriptanalyzer.md