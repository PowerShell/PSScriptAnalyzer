---
description: Avoid using broken hash algorithms
ms.date: 06/01/2026
ms.topic: reference
title: AvoidUsingBrokenHashAlgorithms
---
# AvoidUsingBrokenHashAlgorithms

**Severity Level: Warning**

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
