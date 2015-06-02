[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp", "", Scope="Function", Target="*")]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPositionalParameters", Scope="Function", Target="Test*")]
Param(
)
 
function SuppressMe ()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideVerboseMessage")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideDefaultParameterValue", "unused1")]
    Param([string]$unUsed1, [int] $unUsed2)
    {
        Write-Host "I do nothing"
    }

}

function SuppressTwoVariables()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideDefaultParameterValue", "b")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideDefaultParameterValue", "a")]
    Param([string]$a, [int]$b)
    {
    }
}

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