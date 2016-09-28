#AvoidGlobalFunctions
**Severity Level: Warning**

##Description
Globally scoped functions and alias override existing functions and aliases within the sessions with matching names. This name collision can cause difficult to debug issues for consumers of modules and scripts.  


To understand more about scoping, see ```Get-Help about_Scopes```.

##How to Fix
Use other scope modifiers for functions and aliases.

##Example
###Wrong:
``` PowerShell
function global:functionName {} 

New-Alias -Name CommandName -Value NewCommandAlias -scope:global
```

###Correct:
``` PowerShell
function functionName {} 

New-Alias -Name CommandName -Value NewCommandAlias
```
