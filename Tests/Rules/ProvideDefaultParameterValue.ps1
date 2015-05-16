function BadFunc
{
    param(
        [Parameter()]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1
    )
}

function BadFunc2($Param1)
{
}