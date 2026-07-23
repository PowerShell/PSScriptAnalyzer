---
description: Reserved cmdlet characters
ms.date: 07/21/2026
ms.topic: reference
title: ReservedCmdletChar
---
# ReservedCmdletChar

**Severity Level: Error**

**Default state: Always enabled**

## Description

This rule detects when reserved characters are used in function or cmdlet names. You can't use
reserved characters in a function or cmdlet name as they cause parsing or runtime errors. Remove
reserved characters from your names.

Reserved characters include:

| Character | Meaning                                      |
|:---------:|:---------------------------------------------|
|  `` ` ``  | Escape character (backtick)                  |
|    `$`    | Variable expansion                           |
|    `"`    | String delimiter (allows variable expansion) |
|    `'`    | String delimiter (literal string)            |
|   `( )`   | Grouping expressions / method calls          |
|   `{ }`   | Script blocks / hash tables                  |
|   `[ ]`   | Type literals / array indexing               |
|    `,`    | Array element separator                      |
|    `;`    | Statement separator                          |
|    `&`    | Call operator                                |
|    `?`    | Alias for Where-Object in some contexts      |
|    `*`    | Wildcard                                     |
|   `< >`   | Redirection operators                        |
|    `@`    | Array or hash table literal                  |
|    `:`    | Scope modifier / drive separator             |
|    `#`    | Comment                                      |
|    `=`    | Assignment operator                          |
|    `+`    | String concatenation operator                |
|    `~`    | Expands to the value of the $HOME variable   |
|    `\|`   | Pipe operator                                |

## Example

### Noncompliant

```powershell
function MyFunction[1]
{...}
```

### Compliant

```powershell
function MyFunction
{...}
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
