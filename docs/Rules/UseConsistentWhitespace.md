---
description: Use consistent whitespace in PowerShell code
ms.date: 06/10/2026
ms.topic: reference
title: UseConsistentWhitespace
---
# UseConsistentWhitespace

**Severity Level: Warning**

## Description

This rule detects inconsistent or redundant whitespace around braces, parentheses, operators,
separators, pipes, and parameter values. Use this rule to keep PowerShell code readable and
consistently formatted. This rule is **disabled** by default.

## Example

### Noncompliant

```powershell
if($true) {foo| ForEach-Object{ $_ -eq 1}}
@{a = 1;b = 2}
```

### Compliant

```powershell
if ($true) { foo | ForEach-Object { $_ -eq 1 } }
@{a = 1; b = 2}
```

## Configure rule

```powershell
Rules = @{
    PSUseConsistentWhitespace  = @{
        Enable                          = $true
        CheckInnerBrace                 = $true
        CheckOpenBrace                  = $true
        CheckOpenParen                  = $true
        CheckOperator                   = $true
        CheckPipe                       = $true
        CheckPipeForRedundantWhitespace = $false
        CheckSeparator                  = $true
        CheckParameter                  = $false
        IgnoreAssignmentOperatorInsideHashTable = $false
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks code against this rule. It accepts a boolean
value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### CheckInnerBrace

This parameter checks whether there's a space after an opening brace and a space before a closing
brace. For example, it prefers `if ($true) { foo }` over `if ($true) {bar}`. The default value is
`$true`.

### CheckOpenBrace

This parameter checks whether there's a space between a keyword and its corresponding opening brace.
For example, it prefers `foo { }` over `foo{ }`. No space is required if an open brace precedes an
open parenthesis. The default value is `$true`.

### CheckOpenParen

This parameter checks whether there's a space between a keyword and its corresponding opening
parenthesis. For example, it prefers `if (true)` over `if(true)`. The default value is `$true`.

### CheckOperator

This parameter checks whether binary or unary operators are surrounded by spaces. For example, it
prefers `$x = 1` over `$x=1`. The default value is `$true`.

### CheckSeparator

This parameter checks whether commas and semicolons are followed by a space. For example, it prefers
`@(1, 2, 3)` over `@(1,2,3)` and `@{a = 1; b = 2}` over `@{a = 1;b = 2}`. The default value is
`$true`.

### CheckPipe

This parameter checks whether a pipe is surrounded by a single space on each side, while ignoring
redundant whitespace. For example, it prefers `foo | bar` over `foo|bar`. The default value is
`$true`.

### CheckPipeForRedundantWhitespace

This parameter checks whether a pipe is surrounded by redundant whitespace, meaning more than one
space. For example, it prefers `foo | bar` over `foo    |    bar`. The default value is `$false`.

### CheckParameter

This parameter checks whether there's more than one space between parameters and values. For
example, it prefers `foo -bar $baz -bat` over `foo   -bar   $baz   -bat`, which helps remove
redundant whitespace that was likely added unintentionally. The rule doesn't check whitespace
between a parameter and value when the colon syntax `-ParameterName:$ParameterValue` is used,
because some users prefer either zero or one space in that case. The default value is `$false`.

### IgnoreAssignmentOperatorInsideHashTable

When `CheckOperator` is enabled, this parameter ignores whitespace around assignment operators
within multi-line hash tables. Use it with the `AlignAssignmentStatement` rule when you want to
align assignments in hash tables but still check operator whitespace everywhere else. The default
value is `$false`.
