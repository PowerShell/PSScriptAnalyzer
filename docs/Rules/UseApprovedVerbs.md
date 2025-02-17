---
description: Cmdlet Verbs
ms.date: 03/26/2024
ms.topic: reference
title: UseApprovedVerbs
---
# UseApprovedVerbs

**Severity Level: Warning**

## Description

All cmdlets must used approved verbs.

Approved verbs can be found by running the command `Get-Verb`.

For a more information about approved verbs, see [Approved Verbs for PowerShell Commands][01]. Some
unapproved verbs are documented on the approved verbs page and point to approved alternatives. Try
searching for the verb you used to find its approved form. For example, searching for `Read`,
`Open`, or `Search` leads you to `Get`.

## How

Change the verb in the cmdlet's name to an approved verb.

## Example

### Wrong

```powershell
function Change-Item
{
    ...
}
```

### Correct

```powershell
function Update-Item
{
    ...
}
```

<!-- link references -->
[01]: /powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands
