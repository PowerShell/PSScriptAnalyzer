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
        [int]
        $passwordinteger,
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
    $PassThru,
    [string[]]
    $shouldnotraiseerror
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

function TestFunction([securestring]$password, [System.Security.SecureString[]]$passphrases, [securestring[]]$passes){
}