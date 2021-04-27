function Credential([string]$credential) {

}

# this one is wrong because pscredential should come first
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
        [System.Management.Automation.Credential()]
        [pscredential]
        $Credential
    )
}