function GoodFunc
{
    param(
        [Parameter(Mandatory=$false)]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1=$null
    )
    $Param1
}