# UseApprovedVerbs

**Severity Level: Warning**

## Description

All CMDLets must used approved verbs.

Approved verbs can be found by running the command `Get-Verb`.

Additional documentation on approved verbs can be found in the microsoft docs page [Approved Verbs for PowerShell Commands](https://docs.microsoft.com/powershell/developer/cmdlet/approved-verbs-for-windows-powershell-commands). If you find the verb you are using is unapproved, try searching the page for the approved equivalent. For example, if you search in the documentation for `Read`, `Open`, or `Search` you will find that the approved verb for those situations is `Get`.

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
