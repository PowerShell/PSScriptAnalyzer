---
description: Align assignment statement
ms.date: 06/12/2026
ms.topic: reference
title: AlignAssignmentStatement
---
# AlignAssignmentStatement

**Severity Level: Warning**

## Description

This rule detects misaligned assignment operators in hashtables and enum definitions. Consecutive
assignment statements are easier to read and maintain when their assignment operators align
vertically.

This rule checks key-value pairs in hashtables and enum member definitions to ensure that the `=`
signs line up. Use this rule to enforce consistent formatting in multiline hashtables and enum
definitions.

The rule ignores assignments within hashtables and enums that appear on the same line as other
assignments. For example, the rule ignores `$h = @{ a = 1; b = 2 }`.

## Example

### Noncompliant

```powershell
$hashtable = @{
    property = 'value'
    anotherProperty = 'another value'
}

enum Enum {
    member = 1
    anotherMember = 2
}
```

### Compliant

```powershell
$hashtable = @{
    property        = 'value'
    anotherProperty = 'another value'
}

enum Enum {
    member        = 1
    anotherMember = 2
}
```

## Configuration

```powershell
Rules = @{
    PSAlignAssignmentStatement = @{
        Enable                                  = $true
        CheckHashtable                          = $true
        AlignHashtableKvpWithInterveningComment = $true
        CheckEnum                               = $true
        AlignEnumMemberWithInterveningComment   = $true
        IncludeValuelessEnumMembers             = $true
    }
}
```

## Parameters

### Enable

This parameter controls whether ScriptAnalyzer checks the code against this rule. It accepts a
boolean value. To enable this rule, set this parameter to `$true`. The default value is `$false`.

### CheckHashtable

This parameter controls whether ScriptAnalyzer checks assignment alignment in hashtables and Desired
State Configuration (DSC) configurations. It accepts a boolean value. To disable this check, set
this parameter to `$false`. The default value is `$true`.

### AlignHashtableKvpWithInterveningComment

This parameter controls whether ScriptAnalyzer includes hashtable key-value pairs that contain an
intervening comment when determining alignment. It accepts a boolean value. To exclude these lines,
set this parameter to `$false`. The default value is `$true`.

Consider the following:

```powershell
$hashtable = @{
    property = 'value'
    anotherProperty <#A Comment#> = 'another value'
    anotherDifferentProperty = 'yet another value'
}
```

With this setting disabled, the line with the comment is ignored. The equal signs are aligned for
the remaining lines:

```powershell
$hashtable = @{
    property                 = 'value'
    anotherProperty <#A Comment#> = 'another value'
    anotherDifferentProperty = 'yet another value'
}
```

With this setting enabled, the equal signs are aligned for all lines:

```powershell
$hashtable = @{
    property                      = 'value'
    anotherProperty <#A Comment#> = 'another value'
    anotherDifferentProperty      = 'yet another value'
}
```

### CheckEnum

This parameter controls whether ScriptAnalyzer checks assignment alignment in enum member
definitions. It accepts a boolean value. To disable this check, set this parameter to `$false`. The
default value is `$true`.

### AlignEnumMemberWithInterveningComment

This parameter controls whether ScriptAnalyzer includes enum members that contain an intervening
comment when determining alignment. It accepts a boolean value. To exclude these lines, set this
parameter to `$false`. The default value is `$true`.

### IncludeValuelessEnumMembers

This parameter controls whether ScriptAnalyzer includes enum members without explicitly assigned
values when determining alignment. It accepts a boolean value. To exclude valueless members, set
this parameter to `$false`. The default value is `$true`.

Consider the following:

```powershell
enum Enum {
    member = 1
    anotherMember = 2
    anotherDifferentMember
}
```

With this setting disabled, the third line, which has no value, isn't considered when choosing where
to align assignments.

```powershell
enum Enum {
    member        = 1
    anotherMember = 2
    anotherDifferentMember
}
```

With it enabled, the valueless member is included in alignment as if it had a value:

```powershell
enum Enum {
    member                 = 1
    anotherMember          = 2
    anotherDifferentMember
}
```