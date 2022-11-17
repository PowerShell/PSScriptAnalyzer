try
{
    1/0
}
catch [System.Management.Automation.RuntimeException]
{

}
finally
{
    Write-Host "cleaning up ..."
}