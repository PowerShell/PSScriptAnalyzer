[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp", "", Scope="Function", Target="*")]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPositionalParameters", "", Scope="Function", Target="*")]
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