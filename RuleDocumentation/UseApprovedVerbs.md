#UseApprovedVerbs
**Severity Level: Warning**

##Description
All CMDLets must used approved verbs.

Approved verbs can be found by running the command `Get-Verb`.

##How to Fix
Change the verb in the cmdlet's name to an approved verb.

##Example
###Wrong：
``` PowerShell
function Change-Item
{
    ...
}
````

###Correct:
``` PowerShell
function Update-Item
{
    ...
}
```
