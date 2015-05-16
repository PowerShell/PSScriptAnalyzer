function BadFunc
{
    param(
        [Parameter()]
        [ValidateNotNullOrEmpty()]    
        [string]
        $Param1
    )
}