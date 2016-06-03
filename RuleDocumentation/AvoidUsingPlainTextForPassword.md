#AvoidUsingPlainTextForPassword 
**Severity Level: Warning**


##Description

Password parameters that take in plaintext will expose passwords and compromise the security of your system.

##How to Fix

To fix a violation of this rule, please use SecureString as the type of password parameter.

##Example

Wrong： 
```
    function Test-Script
    {
        [CmdletBinding()]
        [Alias()]
        [OutputType([int])]
        Param
        (
            [string]
            $Password,
            [string]
            $Pass,
            [string[]]
            $Passwords,
            $Passphrases,
            $Passwordparam
        )
        ...
    }
```

Correct: 

```
    function Test-Script
    {
        [CmdletBinding()]
        [Alias()]
        [OutputType([Int])]
        Param
        (
            [SecureString]
            $Password,
            [System.Security.SecureString]
            $Pass,
            [SecureString[]]
            $Passwords,
            [SecureString]
            $Passphrases,
            [SecureString]
            $PasswordParam
        )
        ...
    }

```
