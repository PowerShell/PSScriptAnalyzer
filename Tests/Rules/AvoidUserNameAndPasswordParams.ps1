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

function MyFunction3
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
        [System.Management.Automation.CredentialAttribute()]
        [pscredential]
        $UserName,

        # Param2 help description
        [System.Management.Automation.CredentialAttribute()]
        [pscredential]
        $Password
    )
}

function TestFunction1($password, $username, [PSCredential[]]$passwords){
}

function TestFunction2($username, $password, [PSCredential[]]$passwords){
}
