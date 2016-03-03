function Credential([string]$credential) {

}

# Order between PSCredential and CredentialAttribute should not matter.
function Credential2
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
        $Credential
    )
}