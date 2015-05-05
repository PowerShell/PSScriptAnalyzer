Write-Output "This is the correct way to write output"

# Even if write-host is used, error should not be raised in this function
function Show-Something
{
    Write-Host "show something on screen";
}