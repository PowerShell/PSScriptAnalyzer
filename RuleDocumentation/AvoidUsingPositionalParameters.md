#AvoidUsingPositionalParameters 
**Severity Level: Info**

##Description

To fix a violation of this rule, please use named parameters instead of positional parameters when calling a command.

##How to Fix

To fix a violation of this rule, please use named parameters instead of positional parameters when calling a command.

##Example
Wrong:

	Get-ChildItem *.txt

Correct:

	Get-Content -Path *.txt
