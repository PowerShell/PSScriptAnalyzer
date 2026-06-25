---
description: Avoid trailing whitespace
ms.date: 06/01/2026
ms.topic: reference
title: AvoidTrailingWhitespace
---
# AvoidTrailingWhitespace

**Severity Level: Information**

## Description

This rule detects lines that end with trailing whitespace characters. Lines shouldn't end with
trailing whitespace characters. Trailing whitespace makes diffs harder to review and can introduce
subtle problems when line continuation uses a backtick (`), because the backtick must be the last
character on the line.

Keeping lines free of trailing whitespace improves readability and helps keep source control history
clean.

To learn more, see [about_Parsing][01].

## Example

### Noncompliant

```powershell
# The next line ends with a trailing space after the backtick.
Get-Process `
| Where-Object { $_.CPU -gt 100 }
```

When you run this script, PowerShell throws a parser error because the trailing space prevents line
continuation. For example:

```output
PS C:\WINDOWS\system32> Get-Process `
| Where-Object { $_.CPU -gt 100 }
At line:2 char:1
+ | Where-Object { $_.CPU -gt 100 }
+ ~
An empty pipe element is not allowed.
    + CategoryInfo          : ParserError: (:) [], ParentContainsErrorRecordException
    + FullyQualifiedErrorId : EmptyPipeElement
```

### Compliant

```powershell
Get-Process `
| Where-Object { $_.CPU -gt 100 }
```

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_parsing
