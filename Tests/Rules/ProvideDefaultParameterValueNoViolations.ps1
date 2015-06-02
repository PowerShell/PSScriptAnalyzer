function GoodFunc
{
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1,
        [Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param2=$null
    )
    $Param1
}

function GoodFunc2($Param1 = $null)
{
    $Param1
}