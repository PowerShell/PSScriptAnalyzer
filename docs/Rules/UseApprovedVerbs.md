---
description: Approved cmdlet verbs
ms.date: 07/21/2026
ms.topic: reference
title: UseApprovedVerbs
---
# UseApprovedVerbs

**Severity Level: Warning**

**Default state: Always enabled**

## Description

This rule detects cmdlets that use unapproved verbs in their names. All cmdlets must use approved
verbs. You can find approved verbs by running the `Get-Verb` command. If your cmdlet uses an
unapproved verb, change it to an approved alternative.

Some unapproved verbs are documented on the approved verbs page with suggested alternatives. Try
searching for the verb you're using to find its approved form. For example, `Read`, `Open`, or
`Search` should be replaced with `Get`.

To learn more, see [Approved Verbs for PowerShell Commands][01].

## Example

### Noncompliant

```powershell
function Change-Item {
    ...
}
```

### Compliant

```powershell
function Update-Item {
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
[01]: /powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands
[02]: ../using-scriptanalyzer.md
