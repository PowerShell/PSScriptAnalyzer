#AvoidReservedParams
**Severity Level: Error**


##Description

You cannot use reserved common parameters in an advanced function. If these parameters are defined by the user, an error generally occurs.

##How to Fix

To fix a violation of this rule, please change the name of your parameter.

##Example

Wrong： 
```
function Test
{
    [CmdletBinding]
    Param($ErrorVariable, $b)
}
```

Correct:
```
function Test
{
    [CmdletBinding]
    Param($err, $b)
}
```
