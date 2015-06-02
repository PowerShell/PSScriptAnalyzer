function BadFunc
{
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1,
        [Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param2
    )
    $Param1
    $Param1 = "test"
}

function BadFunc2($Param1)
{
    $Param1
}