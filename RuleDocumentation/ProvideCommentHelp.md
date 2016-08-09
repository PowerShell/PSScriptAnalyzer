#ProvideCommentHelp 
**Severity Level: Info**

##Description
Comment based help should be provided for all PowerShell commands. This test only checks for the presence of comment based help and not on the validity or format.

For assistance on comment based help, use the command ```Get-Help about_comment_based_help``` or the article, "How to Write Cmdlet Help" (http://go.microsoft.com/fwlink/?LinkID=123415).

##How to Fix
Include comment based help for each command identified.

##Example
###Wrong:
``` PowerShell
function Get-File
{
    [CmdletBinding()]
    Param
    (
        ...
    )
    
}
```

###Correct:
``` PowerShell
<#
.Synopsis
    Short description
.DESCRIPTION
    Long description
.EXAMPLE
    Example of how to use this cmdlet
.EXAMPLE
    Another example of how to use this cmdlet
.INPUTS
    Inputs to this cmdlet (if any)
.OUTPUTS
    Output from this cmdlet (if any)
.NOTES
    General notes
.COMPONENT
    The component this cmdlet belongs to
.ROLE
    The role this cmdlet belongs to
.FUNCTIONALITY
    The functionality that best describes this cmdlet
#>
function Get-File
{
    [CmdletBinding()]
    Param
    (
        ...
    )
    
}
```
