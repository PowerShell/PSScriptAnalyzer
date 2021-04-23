function GoodFuncCmdletBinding
{
    [CmdletBinding()]
    param(
        # this one null value
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param1="String",
        
        # this parameter has no help message
        [Parameter(HelpMessage="This is helpful too.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}

function GoodFunc1($Param1)
{
    $Param1
}

# same as BadFunc but this one has no cmdletbinding
function GoodFuncNoCmdletBinding
{
    param(
        # this one null value
        [Parameter(HelpMessage="This is helpful.")]          
        [string] $Param1="String",
        
        # this parameter has no help message
        [Parameter(HelpMessage="This is helpful too.")]          
        [string] $Param2
    )
    $Param1
    $Param2 = "test"
}
