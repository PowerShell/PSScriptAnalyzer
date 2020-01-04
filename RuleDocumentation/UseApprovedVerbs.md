# UseApprovedVerbs

**Severity Level: Warning**

## Description

All CMDLets must used approved verbs.

Approved verbs can be found by running the command `Get-Verb`.

Additional documentation on approved verbs can be found in the microsoft docs page
[Approved Verbs for PowerShell Commands](https://docs.microsoft.com/powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands).
Some unapproved verbs are documented on the approved verbs page and point to approved alternatives;
try searching for the verb you used to find its approved form.
For example, searching for `Read`, `Open`, or `Search` will lead you to `Get`.

## How

Change the verb in the cmdlet's name to an approved verb.

## Example

### Wrong

``` PowerShell
function Change-Item
{
    ...
}
````

### Correct

``` PowerShell
function Update-Item
{
    ...
}
```
