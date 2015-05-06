#AvoidUsingWriteHost 
**Severity Level: Warning**


##Description

It is generally accepted that you should never use Write-Host to create any script output whatsoever, unless your script (or function, or whatever) uses the Show verb (as in, Show-Performance). That verb explicitly means “show on the screen, with no other possibilities.” Like Show-Command.

##How to Fix

PTo fix a violation of this rule, please replace Write-Host with Write-Output.

##Example

Wrong： 

```
function Test
{
	...
	Write-Host "Executing.."
}
```

Correct: 

```
function Test
{
	...
	Write-Output "Executing.."
}

function Show-Something
{
    Write-Host "show something on screen";
}
```
