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
                  HelpUri = 'https://www.microsoft.com/',
                  ConfirmImpact='Medium')]
    [Alias()]
    [OutputType([String], [System.Double], [Hashtable], "MyCustom.OutputType")]
    [OutputType("System.Int32", ParameterSetName="ID")]

    Param
    (
        # Param1 help description
        [Parameter(Mandatory=$true,
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ValueFromRemainingArguments=$false,
                   Position=0,
                   ParameterSetName='Parameter Set 1')]
        [ValidateNotNull()]
        [ValidateNotNullOrEmpty()]
        [ValidateCount(0,5)]
        [ValidateSet("sun", "moon", "earth")]
        [Alias("p1")]
        $Param1,

        # Param2 help description
        [Parameter(ParameterSetName='Parameter Set 1')]
        [AllowNull()]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [ValidateScript({$true})]
        [ValidateRange(0,5)]
        [int]
        $Param2,

        # Param3 help description
        [Parameter(ParameterSetName='Another Parameter Set')]
        [ValidatePattern("[a-z]*")]
        [ValidateLength(0,15)]
        [String]
        $Param3,
        [bool]
        $Force
    )

    Begin
    {
    }
    Process
    {
        if ($pscmdlet.ShouldProcess("Target", "Operation"))
        {
            Write-Verbose "Write Verbose"
            Get-Process
        }

        $a = 4.5

        if ($true)
        {
            $a
        }

        $a | Write-Warning

        $b = @{"hash"="table"}

        Write-Debug @b

        [pscustomobject]@{
            PSTypeName = 'MyCustom.OutputType'
            Prop1 = 'SomeValue'
            Prop2 = 'OtherValue'
        }

        return @{"hash"="true"}
    }
    End
    {
        if ($pscmdlet.ShouldContinue("Yes", "No")) {
        }
        [System.Void] $Param3
    }
}

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
function Get-Folder
{
    [CmdletBinding(DefaultParameterSetName='Parameter Set 1',
                  SupportsShouldProcess,
                  PositionalBinding=$false,
                  HelpUri = 'https://www.microsoft.com/',
                  ConfirmImpact='Medium')]
    [Alias()]
    [OutputType([String], [System.Double], [Hashtable], "MyCustom.OutputType")]
    [OutputType("System.Int32", ParameterSetName="ID")]

    Param
    (
        # Param1 help description
        [Parameter(Mandatory=$true,
                   ValueFromPipeline=$true,
                   ValueFromPipelineByPropertyName=$true,
                   ValueFromRemainingArguments=$false,
                   Position=0,
                   ParameterSetName='Parameter Set 1')]
        [ValidateNotNull()]
        [ValidateNotNullOrEmpty()]
        [ValidateCount(0,5)]
        [ValidateSet("sun", "moon", "earth")]
        [Alias("p1")]
        $Param1,

        # Param2 help description
        [Parameter(ParameterSetName='Parameter Set 1')]
        [AllowNull()]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [ValidateScript({$true})]
        [ValidateRange(0,5)]
        [int]
        $Param2,

        # Param3 help description
        [Parameter(ParameterSetName='Another Parameter Set')]
        [ValidatePattern("[a-z]*")]
        [ValidateLength(0,15)]
        [String]
        $Param3,
        [bool]
        $Force
    )

    Begin
    {
    }
    Process
    {
        if ($pscmdlet.ShouldProcess("Target", "Operation"))
        {
            Write-Verbose "Write Verbose"
            Get-Process
        }

        $a = 4.5

        if ($true)
        {
            $a
        }

        $a | Write-Warning

        $b = @{"hash"="table"}

        Write-Debug @b

        [pscustomobject]@{
            PSTypeName = 'MyCustom.OutputType'
            Prop1 = 'SomeValue'
            Prop2 = 'OtherValue'
        }

        return @{"hash"="true"}
    }
    End
    {
        if ($pscmdlet.ShouldContinue("Yes", "No")) {
        }
        [System.Void] $Param3
    }
}

#Write-Verbose should not be required because this is not an advanced function
# use reserved param here
function global:Get-SimpleFunc*
{

}

function Local:Get-SimpleFunc*
{

}

function PRIVATE:Get-SimpleFunc*
{

}

function ScRiPt:Get-SimpleFunc*
{

}

<#
.Synopsis
   Short description
.DESCRIPTION
   Long description
.EXAMPLE
   Example of how to use this cmdlet
.EXAMPLE
   Another example of how to use this cmdlet
#>
function Get-Reserved*
{
    [CmdletBinding()]
    [Alias()]
    [OutputType([int])]
    Param
    (
        # Param1 help description
        [Parameter(Mandatory=$true,
                   ValueFromPipelineByPropertyName=$true,
                   Position=0)]
        $Param1,

        # Param2 help description
        [int]
        $Param2
    )

    Begin
    {
    }
    Process
    {
    }
    End
    {
    }
}

<#
.Synopsis
   function that has a noun that is singular
.DESCRIPTION

.EXAMPLE

.EXAMPLE

#>
function Get-MyWidgetStatus
{

}

function Get-MyFood
{
    [CmdletBinding(SupportsShouldProcess=$true)]
    param([Switch]$Force)

    process
    {
        if ($PSCmdlet.ShouldProcess(("Are you sure?")))
        {
        }
    }
}