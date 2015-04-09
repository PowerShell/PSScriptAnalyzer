#AvoidShouldContinueWithoutForce
**Severity Level: Warning**


##Description

Functions that use ShouldContinue should have a boolean force parameter to allow user to bypass it.

##How to Fix

To fix a violation of this rule, please call ShouldContinue method in advanced functions when ShouldProcess method returns $true. You can get more details by running “Get-Help about_Functions_CmdletBindingAttribute” and “Get-Help about_Functions_Advanced_Methods” command in Windows PowerShell.

##Example
Wrong:

	function Verb-Noun
	{
	    [CmdletBinding(DefaultParameterSetName='Parameter Set 1', 
	                  SupportsShouldProcess=$true, 
	                  PositionalBinding=$false,
	                  HelpUri = 'http://www.microsoft.com/',
	                  ConfirmImpact='Medium')]
	    [Alias()]
	    [OutputType([string])]
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
	        [string]
	        $Param3
	    )

	    Begin
	    {
	        $pscmdlet.ShouldContinue("Yes", "No")
	    }
	    Process
	    {
	        if ($pscmdlet.ShouldProcess("Target", "Operation"))
	        {
	        }
	    }
	    End
	    {
	    }
	}

Correct:

	function Get-File
	{
	    [CmdletBinding(DefaultParameterSetName='Parameter Set 1', 
	                  SupportsShouldProcess=$true, 
	                  PositionalBinding=$false,
	                  HelpUri = 'http://www.microsoft.com/',
	                  ConfirmImpact='Medium')]
	    [Alias()]
	    [OutputType([string])]
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
	        [string]
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
	    }
	    End
	    {
	        if ($pscmdlet.ShouldContinue("Yes", "No")) {
	        }
	    }
	}
