function MyFunction ($username, $param2)
{
    
}

function MyFunction2 ($param1, $passwords)
{
    
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
        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        $UserName,

        # Param2 help description
        [pscredential]
        [System.Management.Automation.CredentialAttribute()]
        $Password
    )
}

function MyFunction3
{
    param(
    [string] $Username,
    [switch] $HidePassword
    )
}

function MyFunction4
{
    param(
    [string] $Username,
    [bool] $HidePassword
    )
}