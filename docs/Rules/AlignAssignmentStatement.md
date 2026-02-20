---
description: Align assignment statement
ms.date: 06/28/2023
ms.topic: reference
title: AlignAssignmentStatement
---
# AlignAssignmentStatement

**Severity Level: Warning**

## Description

Consecutive assignment statements are more readable when they're aligned.
Assignments are considered aligned when their `equals` signs line up vertically.

This rule looks at the key-value pairs in hashtables (including DSC
configurations) as well as enum definitions.

Consider the following example which has a hashtable and enum which are not
aligned.

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

Alignment in this case would look like the following.

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

The rule ignores any assignments within hashtables and enums which are on the
same line as others. For example, the rule ignores `$h = @{a = 1; b = 2}`.

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

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### CheckHashtable: bool (Default value is `$true`)

Enforce alignment of assignment statements in a hashtable and in a DSC
Configuration. There is only one setting for hashtable and DSC configuration
because the property value pairs in a DSC configuration are parsed as key-value
pairs of a hashtable.

#### AlignHashtableKvpWithInterveningComment: bool (Default value is `$true`)

Include key-value pairs in the alignment that have an intervening comment - that
is to say a comment between the key name and the equals sign.

Consider the following:

```powershell
$hashtable = @{
    property = 'value'
    anotherProperty <#A Comment#> = 'another value'
    anotherDifferentProperty = 'yet another value'
}
```

With this setting disabled, the line with the comment is ignored, and it would
be aligned like so:

```powershell
$hashtable = @{
    property                 = 'value'
    anotherProperty <#A Comment#> = 'another value'
    anotherDifferentProperty = 'yet another value'
}
```

With it enabled, the comment line is included in alignment:

```powershell
$hashtable = @{
    property                      = 'value'
    anotherProperty <#A Comment#> = 'another value'
    anotherDifferentProperty      = 'yet another value'
}
```

#### CheckEnum: bool (Default value is `$true`)

Enforce alignment of assignment statements of an Enum definition.

#### AlignEnumMemberWithInterveningComment: bool (Default value is `$true`)

Include enum members in the alignment that have an intervening comment - that
is to say a comment between the member name and the equals sign.

Consider the following:

```powershell
enum Enum {
    member = 1
    anotherMember <#A Comment#> = 2
    anotherDifferentMember = 3
}
```

With this setting disabled, the line with the comment is ignored, and it would
be aligned like so:

```powershell
enum Enum {
    member                 = 1
    anotherMember <#A Comment#> = 2
    anotherDifferentMember = 3
}
```

With it enabled, the comment line is included in alignment:

```powershell
enum Enum {
    member                      = 1
    anotherMember <#A Comment#> = 2
    anotherDifferentMember      = 3
}
```

#### IncludeValuelessEnumMembers: bool (Default value is `$true`)

Include enum members in the alignment that don't have an initial value - that
is to say they don't have an equals sign. Enum's don't need to be given a value
when they're defined.

Consider the following:

```powershell
enum Enum {
    member = 1
    anotherMember = 2
    anotherDifferentMember
}
```

With this setting disabled the third line which has no value is not considered
when choosing where to align assignments. It would be aligned like so:

```powershell
enum Enum {
    member        = 1
    anotherMember = 2
    anotherDifferentMember
}
```

With it enabled, the valueless member is included in alignment as if it had a
value:

```powershell
enum Enum {
    member                 = 1
    anotherMember          = 2
    anotherDifferentMember
}
```