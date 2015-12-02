function MyFunction ($username, $param2)
{
    
}

function MyFunction2 ($param1, $passwords)
{
    
}

function MyFunction3 ([PSCredential]$username, $passwords)
{
}

function MyFunction4
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
        $UserName,

        # Param2 help description
        [int]
        [System.Management.Automation.CredentialAttribute()]
        $Password
    )
}
