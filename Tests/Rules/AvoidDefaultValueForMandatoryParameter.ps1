function BadFunc
{
    [CmdletBinding()]
    param(
        # this one has default value
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1="String",
        # this parameter has no default value
        [Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param2
    )
    $Param1
    $Param1 = "test"
}

function GoodFunc1($Param1)
{
    $Param1
}

# same as BadFunc but this one has no cmdletbinding
function GoodFunc2
{
    param(
        # this one has default value
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1="String",
        # this parameter has no default value
        [Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param2
    )
    $Param1
    $Param1 = "test"
}