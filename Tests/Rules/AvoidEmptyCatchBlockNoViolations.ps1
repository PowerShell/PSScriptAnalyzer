try
{
    1/0
}
catch [DivideByZeroException]
{
    Write-Host "Divide by zero exception"
}
catch [System.Net.WebException],[System.Exception]
{
    Write-Host "Other exception"
}
finally
{
    Write-Host "cleaning up ..."
}
