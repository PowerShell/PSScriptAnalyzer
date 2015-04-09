#AvoidUsingUsernameAndPasswordParams
**Severity Level: Error**


##Description

Functions should only take in a credential parameter of type PSCredential instead of username and password parameters.

##How to Fix

To fix a violation of this rule, please pass username and password as a PSCredential type parameter.

##Example

Wrong：    
```
    [Int]
    $Param2,
    [SecureString]
    $Password,
    [String]
    $Username
```

Correct:   
```
    function MyFunction3 ([PSCredential]$Username, $Passwords)
    {
    }
```
