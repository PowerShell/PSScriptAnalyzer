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
        [string]
        $username
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

function TestFunction($password, [PSCredential[]]$passwords, $username){
}