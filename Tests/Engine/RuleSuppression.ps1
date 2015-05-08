[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp", "", Scope="Function", Target="*")]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPositionalParameters", "", Scope="Function", Target="*")]
Param(
)
 
function SuppressMe ()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideVerboseMessage", "")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUninitializedVariable", "unused1")]
    Param([string]$unUsed1, [int] $unUsed2)
    {
        Write-Host "I do nothing"
    }

}