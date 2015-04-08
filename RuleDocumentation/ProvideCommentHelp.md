#ProvideCommentHelp 
**Severity Level: Info**


##Description

Checks that all cmdlets have a help comment. This rule only checks existence. It does not check the content of the comment.


##How to Fix

Please consider adding help comment for each cmdlet.

##Example

Wrong:

    function Verb-Files
    {
        [CmdletBinding(DefaultParameterSetName='Parameter Set 1', 
                      SupportsShouldProcess=$true, 
                      PositionalBinding=$false,
                      HelpUri = 'http://www.microsoft.com/',
                      ConfirmImpact='Medium')]
        [Alias()]
        [OutputType([String])]
        Param
        (
            ...
        )
       
    }

Right:

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
        [CmdletBinding(DefaultParameterSetName='Parameter Set 1', 
                      SupportsShouldProcess=$true, 
                      PositionalBinding=$false,
                      HelpUri = 'http://www.microsoft.com/',
                      ConfirmImpact='Medium')]
        [Alias()]
        [OutputType([String])]
        Param
        (
            ...
        )
    }