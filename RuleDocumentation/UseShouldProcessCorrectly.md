#UseShouldProcessCorrectly 
**Severity Level: Warning**


##Description

Checks that if the SupportsShouldProcess is present, the function calls ShouldProcess/ShouldContinue and vice versa. Scripts with one or the other but not both will generally run into an error or unexpected behavior.


##How to Fix

To fix a violation of this rule, please call ShouldProcess method in advanced functions when SupportsShouldProcess argument is present. Or please add SupportsShouldProcess argument when calling ShouldProcess.You can get more details by running “Get-Help about_Functions_CmdletBindingAttribute” and “Get-Help about_Functions_Advanced_Methods” command in Windows PowerShell.

##Example

Wrong： 

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
	        $Verbose,
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

Correct:

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
	    }
	    End
	    {
	        if ($pscmdlet.ShouldContinue("Yes", "No")) {
	        }
	    }
	}
