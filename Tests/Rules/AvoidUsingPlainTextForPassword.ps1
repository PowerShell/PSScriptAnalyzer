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
	$passwordparam,
    $credential,
    $auth
    )

    Begin
    {
    }
    Process
    {
    }
    End
    {
    }
}

function TestFunction($password, [System.Security.SecureString[]]$passphrases, [string]$passThru){
}