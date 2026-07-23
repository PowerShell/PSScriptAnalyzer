---
description: Avoid trailing whitespace
ms.date: 07/21/2026
ms.topic: reference
title: AvoidTrailingWhitespace
---
# AvoidTrailingWhitespace

**Severity Level: Information**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- Link references -->
[01]: /powershell/module/microsoft.powershell.core/about/about_parsing
[02]: ../using-scriptanalyzer.md
