#AvoidUsingUsernameAndPasswordParams
**Severity Level: Error**

##Description
To standardize command parameters, credentials should be accept as objects of type ```PSCredential```. Functions should not make use of username or password parameters.

##How to Fix
Change the parameter to type ```PSCredential```.

##Example
###Wrong：    
``` PowerShell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [String]
        $Username,
        [SecureString]
        $Password
    )
    ...
}
```

###Correct:   
``` PowerShell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [PSCredential]
        $Credential
    )
    ...
}
```
