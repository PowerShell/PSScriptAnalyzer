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
        [switch]
        $switch=$true,
        
        # Param3 help description
        [System.Management.Automation.SwitchParameter]
        $switch2 = $true
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