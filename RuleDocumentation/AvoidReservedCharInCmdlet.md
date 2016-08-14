#AvoidReservedCharInCmdlet
**Severity Level: Error**

##Description
You cannot use following reserved characters in a function or CMDLet name as these can cause parsing or runtime errors.

Reserved Characters include: ```#,(){}[]&/\\$^;:\"'<>|?@`*%+=~``` 

##How to Fix
Remove reserved characters from names.

##Example
###Wrong： 
``` PowerShell
function MyFunction[1]
{...}
```

###Correct:
``` PowerShell
function MyFunction
{...}
```
