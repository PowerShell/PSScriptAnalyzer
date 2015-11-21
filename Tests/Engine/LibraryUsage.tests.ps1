Import-Module PSScriptAnalyzer

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path

# Overwrite Invoke-ScriptAnalyzer with a version that
# wraps the usage of ScriptAnalyzer as a .NET library 
function Invoke-ScriptAnalyzer {
	param (
        [CmdletBinding(DefaultParameterSetName="File")]

		[parameter(Mandatory = $true, Position = 0, ParameterSetName="File")]
		[Alias("PSPath")]
		[string] $Path,

		[parameter(Mandatory = $true, ParameterSetName="ScriptDefinition")]
		[string] $ScriptDefinition,

        [Parameter(Mandatory = $false)]
		[Alias("CustomizedRulePath")]
		[string] $CustomRulePath = $null,

		[Parameter(Mandatory = $false)]
		[switch] $RecurseCustomRulePath,

        [Parameter(Mandatory=$false)]
        [string[]] $ExcludeRule = $null,

        [Parameter(Mandatory = $false)]
        [string[]] $IncludeRule = $null, 

        [ValidateSet("Warning", "Error", "Information", IgnoreCase = $true)]
        [Parameter(Mandatory = $false)]
        [string[]] $Severity = $null,

        [Parameter(Mandatory = $false)]
		[switch] $Recurse,

        [Parameter(Mandatory = $false)]
        [switch] $SuppressedOnly
	)
	[string[]]$customRulePathArr = @($CustomRulePath);
	$scriptAnalyzer = New-Object "Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer"
	$scriptAnalyzer.Initialize(
		$runspace, 
		$testOutputWriter, 
		$customRulePathArr, 
		$IncludeRule,
		$ExcludeRule,
		$Severity,
		$SuppressedOnly.IsPresent
	);

    if ($PSCmdlet.ParameterSetName -eq "File") {
    	return $scriptAnalyzer.AnalyzePath($Path, $Recurse.IsPresent);
    }
    else {
        return $scriptAnalyzer.AnalyzeScriptDefinition($ScriptDefinition);
    }
}

# Define an implementation of the IOutputWriter interface
Add-Type -Language CSharp @"
using System.Management.Automation;
using System.Management.Automation.Host;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;

public class PesterTestOutputWriter : IOutputWriter 
{
	private PSHost psHost;

	public string MostRecentWarningMessage { get; private set; }

	public static PesterTestOutputWriter Create(PSHost psHost)
	{
		PesterTestOutputWriter testOutputWriter = new PesterTestOutputWriter();
		testOutputWriter.psHost = psHost;
		return testOutputWriter;
	}

	public void WriteError(ErrorRecord error)
	{
		// We don't write errors to avoid misleading
		// error messages in test output
	}

	public void WriteWarning(string message)
	{
		psHost.UI.WriteWarningLine(message);

		this.MostRecentWarningMessage = message;
	}

	public void WriteVerbose(string message)
	{
		// We don't write verbose output to avoid lots
		// of unnecessary messages in test output
	}

	public void WriteDebug(string message)
	{
		psHost.UI.WriteDebugLine(message);
	}

	public void ThrowTerminatingError(ErrorRecord record)
	{
		throw new RuntimeException(
			"Test failed due to terminating error: \r\n" + record.ToString(), 
			null, 
			record);
	}
}
"@ -ReferencedAssemblies "Microsoft.Windows.PowerShell.ScriptAnalyzer" -ErrorAction SilentlyContinue

if ($testOutputWriter -eq $null)
{
	$testOutputWriter = [PesterTestOutputWriter]::Create($Host);
}

# Create a fresh runspace to pass into the ScriptAnalyzer class
$initialSessionState = [System.Management.Automation.Runspaces.InitialSessionState]::CreateDefault2();
$runspace = [System.Management.Automation.Runspaces.RunspaceFactory]::CreateRunspace([System.Management.Automation.Host.PSHost]$Host, [System.Management.Automation.Runspaces.InitialSessionState]$initialSessionState);
$runspace.Open();

# Let other test scripts know we are testing library usage now
$testingLibraryUsage = $true

# Invoke existing test files that use Invoke-ScriptAnalyzer
. $directory\InvokeScriptAnalyzer.tests.ps1
. $directory\RuleSuppression.tests.ps1
. $directory\CustomizedRule.tests.ps1

# We're done testing library usage
$testingLibraryUsage = $false

# Clean up the test runspace
$runspace.Dispose();

# Re-import the PSScriptAnalyzer module to overwrite the library test cmdlet
Import-Module PSScriptAnalyzer
