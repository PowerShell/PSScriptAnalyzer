#AvoidUsingPing
**Severity Level: Warning**


##Description

Avoid using the Ping command. 

Test-NetConnection and Test-Connection can directly replace ping and do not require parsing text, using $LASTERRORCODE, or the .Net ping class.

##How to Fix

Use Test-NetConnection or Test-Connection. 

##Example

Wrong:
```
$null = ping 127.0.0.1

if($LASTEXITCODE -eq 0)
{
	Write-output "Do Work"
}
```
Correct:
```
if(Test-NetConnection 127.0.0.1)
{
	Write-output "Do Work"
}
```
