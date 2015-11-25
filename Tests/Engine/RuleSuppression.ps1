[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp", "", Scope="Function", Target="*")]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPositionalParameters", Scope="Function", Target="Test*")]
Param(
)
 
function SuppressMe ()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidDefaultValueForMandatoryParameter", "unused1")]
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true)]
        [string]
        $unUsed1="unused",
        
        [Parameter(Mandatory=$true)]
        [int]
        $unUsed2=3
        )
    {
        Write-Host "I do nothing"
    }

}

function SuppressTwoVariables()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidDefaultValueForMandatoryParameter", "b")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidDefaultValueForMandatoryParameter", "a")]
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true)]
        [string]
        $a="unused",

        [Parameter(Mandatory=$true)]
        [int]
        $b=3
        )
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