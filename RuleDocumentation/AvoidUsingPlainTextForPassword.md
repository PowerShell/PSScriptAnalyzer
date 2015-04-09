#AvoidUsingPlainTextForPassword 
**Severity Level: Warning**


##Description

Password parameters that take in plaintext will expose passwords and compromise the security of your system.

##How to Fix

To fix a violation of this rule, please use SecurityString as the type of password parameter.

##Example

Wrong： 
```
    function Verb-Noun
    {
        [CmdletBinding()]
        [Alias()]
        [OutputType([int])]
        Param
        (
            # Param1 help description
            [Parameter(Mandatory=$true,
                       ValueFromPipelineByPropertyName=$true,
                       Position=0)]
            $Param1,
            # Param2 help description
            [Int]
            $Param2,
            [SecureString]
            $Password,
            [System.Security.SecureString]
            $Pass,
            [SecureString[]]
            $Passwords,
            $Passphrases,
            $Passwordparam
        )
    }

    function TestFunction($password, [System.Security.SecureString[]]passphrases, [String]$passThru){
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
	        # Param1 help description
	        [Parameter(Mandatory=$true,
	                   ValueFromPipelineByPropertyName=$true,
	                   Position=0)]
	        $Param1,
	        # Param2 help description
	        [Int]
	        $Param2,
	        [SecureString]
	        $Password,
	        [System.Security.SecureString]
	        $Pass,
		[SecureString[]]
	        $Passwords,
	        [SecureString]
    		$Passphrases,
    	    	[SecureString]
    		$PasswordParam,
    	    	[String]
    	    	$PassThru
    	    )
    	    ...
	}

	function TestFunction([SecureString]$Password, [System.Security.SecureString[]]$Passphrases, [SecureString[]]$passes){
	}
```
