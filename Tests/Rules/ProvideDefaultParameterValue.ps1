function BadFunc
{
    param(
        [Parameter()]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1
    )
    $Param1
    $Param1 = "test"
}

function BadFunc2($Param1)
{
    $Param1
}