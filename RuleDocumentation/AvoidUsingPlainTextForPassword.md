#AvoidUsingPlainTextForPassword 
**Severity Level: Warning**


##Description

Password parameters that take in plaintext will expose passwords and compromise the security of your system.

##How to Fix

To fix a violation of this rule, please use SecurityString as the type of password parameter.

##Example

Wrong： 

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
            [int]
            $Param2,
            [securestring]
            $Password,
            [System.Security.SecureString]
            $pass,
            [securestring[]]
            $passwords,
            $passphrases,
            $passwordparam
        )
    }

    function TestFunction($password, [System.Security.SecureString[]]passphrases, [string]$passThru){
    }


Correct: 

	function Test-Script
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
	        [int]
	        $Param2,
	        [securestring]
	        $Password,
	        [System.Security.SecureString]
	        $pass,
		    [securestring[]]
	        $passwords,
	        [securestring]
    		$passphrases,
    	    [securestring]
    		$passwordparam,
    	    [string]
    	    $PassThru
    	    )
    	    ...
	}

	function TestFunction([securestring]$password, [System.Security.SecureString[]]$passphrases, [securestring[]]$passes){
	}