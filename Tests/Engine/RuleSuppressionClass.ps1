[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "", Scope="Class")]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("pSAvoidUsingInvokeExpression", "")]
class TestClass 
{
    [void] TestFunction2()
    {
        Write-Host "Should not use positional parameters"
        $a = ConvertTo-SecureString -AsPlainText "Test" -Force
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", "")]
    [void] TestFunction()
    {
        Write-Host "Should not use write-host!"
        Invoke-Expression "invoke expression"   
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingCmdletAliases","")]
    [bool] Suppress()
    {
        gps
        return $true
    }
}