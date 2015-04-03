#AvoidReservedCharInCmdlet
**Severity Level: Error**


##Description

You cannot use following reserved characters in a function name. These characters usually cause a parsing error. Otherwise they will generally cause runtime errors.
#,(){}[]&/\\$^;:\"'<>|?@`*%+=~


##How to Fix

To fix a violation of this rule, please remove reserved characters from your advanced function name.

##Example

Wrong： 

function test[1]
{...}

Correct:

function test
{...}