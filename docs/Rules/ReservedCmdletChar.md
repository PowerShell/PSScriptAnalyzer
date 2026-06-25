---
description: Reserved cmdlet characters
ms.date: 06/08/2026
ms.topic: reference
title: ReservedCmdletChar
---
# ReservedCmdletChar

**Severity Level: Error**

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
