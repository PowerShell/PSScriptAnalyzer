function Use-#Reserved
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
        $Param2
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

function O
{
    [CmdletBinding()]
    [Alias()]
    [OutputType([int])]
    Param()

    Write-Output "I use one char"
}

Export-ModuleMember Use-#Reserved
Export-ModuleMember O