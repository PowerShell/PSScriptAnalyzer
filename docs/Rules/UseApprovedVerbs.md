---
description: Approved cmdlet verbs
ms.date: 06/08/2026
ms.topic: reference
title: UseApprovedVerbs
---
# UseApprovedVerbs

**Severity Level: Warning**

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

<!-- link references -->
[01]: https://learn.microsoft.com/powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands
