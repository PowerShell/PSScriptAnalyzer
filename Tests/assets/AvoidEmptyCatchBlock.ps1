try
{
    1/0
}
catch [DivideByZeroException]
{
}
catch [System.Net.WebException],[System.Exception]
{
}
finally
{
    Write-Host "cleaning up ..."
}