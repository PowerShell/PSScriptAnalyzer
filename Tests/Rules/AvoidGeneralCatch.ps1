try
{
    1/0
}
catch [DivideByZeroException]
{
    "catch divide by zero exception"
}
catch [System.Management.Automation.RuntimeException]
{
    "catch RuntimeException"
}
catch
{
    "No exception"
}
finally
{
    "cleaning up ..."
}