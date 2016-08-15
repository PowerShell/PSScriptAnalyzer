<#
.SYNOPSIS
Tests the PowerShell help for the commands in a module.

.DESCRIPTION
This Pester test verifies that the commands in a module have basic help content.
It works on all command types and both comment-based and XML help.

This test verifies that Get-Help is not autogenerating help because it cannot
find any help for the command. Then, it checks for the following help elements:
	- Synopsis
	- Description
	- Parameter:
		- A description of each parameter.
		- An accurate value for the Mandatory property.
		- An accurate value for the .NET type of the parameter value.
	- No extra parameters:
		- Verifies that there are no parameters in help that are not also in the code.

When testing attributes of parameters that appear in multiple parameter sets,
this test uses the parameter that appears in the default parameter set, if one
is defined.

You can run this Tests file from any location. For a help test that is located in a module
directory, use https://github.com/juneb/PesterTDD/InModule.Help.Tests.ps1

.PARAMETER ModuleName
Enter the name of the module to test. You can enter only one name at a time. This
parameter is mandatory.

.PARAMETER RequiredVersion
Enter the version of the module to test. This parameter is optional. If you
omit it, the test runs on the latest version of the module in $env:PSModulePath.

.EXAMPLE
.\Module.Help.Tests.ps1 -ModuleName Pester -RequiredVersion 3.4.0
This command runs the tests on the commands in Pester 3.4.0.

.EXAMPLE
.\Module.Help.Tests.ps1 -ModuleName Pester
This command runs the tests on the commands in latest local version of the
Pester module.


.NOTES
	===========================================================================
	Created with: 	SAPIEN Technologies, Inc., PowerShell Studio 2016 v5.2.119
	Created on:   	4/12/2016 1:11 AM
	Created by:   	June Blender
	Organization: 	SAPIEN Technologies, Inc
	Filename:		*.Help.Tests.ps1
	===========================================================================
#>

Param
(
	[ValidateScript({ Get-Module -ListAvailable -Name $_ })]
	[string]
	$ModuleName = 'PSScriptAnalyzer',

	[Parameter(Mandatory = $false)]
	[System.Version]
	$RequiredVersion
)

# #Requires -Module @{ModuleName = 'Pester'; ModuleVersion = '3.4.0'}

<#
.SYNOPSIS
Gets command parameters; one per name. Prefers default parameter set.

.DESCRIPTION
Gets one CommandParameterInfo object for each parameter in the specified
command. If a command has more than one parameter with the same name, this
function gets the parameters in the default parameter set, if one is specified.

For example, if a command has two parameter sets:
	Name, ID  (default)
	Name, Path
This function returns:
    Name (default), ID Path

This function is used to get parameters for help and for help testing.

.PARAMETER Command
Enter a CommandInfo object, such as the object that Get-Command returns. You
can also pipe a CommandInfo object to the function.

This parameter takes a CommandInfo object, instead of a command name, so
you can use the parameters of Get-Command to specify the module and version
of the command.

.EXAMPLE
PS C:\> Get-ParametersDefaultFirst -Command (Get-Command New-Guid)
This command uses the Command parameter to specify the command to
Get-ParametersDefaultFirst

.EXAMPLE
PS C:\> Get-Command New-Guid | Get-ParametersDefaultFirst
You can also pipe a CommandInfo object to Get-ParametersDefaultFirst

.EXAMPLE
PS C:\> Get-ParametersDefaultFirst -Command (Get-Command BetterCredentials\Get-Credential)
You can use the Command parameter to specify the CommandInfo object. This
command runs Get-Command module-qualified name value.

.EXAMPLE
PS C:\> $ModuleSpec = @{ModuleName='BetterCredentials';RequiredVersion=4.3}
PS C:\> Get-Command -FullyQualifiedName $ModuleSpec | Get-ParametersDefaultFirst
This command uses a Microsoft.PowerShell.Commands.ModuleSpecification object to
specify the module and version. You can also use it to specify the module GUID.
Then, it pipes the CommandInfo object to Get-ParametersDefaultFirst.
#>
function Get-ParametersDefaultFirst {
	Param
	(
		[Parameter(Mandatory = $true,
				   ValueFromPipeline = $true)]
		[System.Management.Automation.CommandInfo]
		$Command
	)

	BEGIN {
		$Common = 'Debug', 'ErrorAction', 'ErrorVariable', 'InformationAction', 'InformationVariable', 'OutBuffer', 'OutVariable', 'PipelineVariable', 'Verbose', 'WarningAction', 'WarningVariable'
		$parameters = @()
	}
	PROCESS {
		if ($defaultPSetName = $Command.DefaultParameterSet) {
			$defaultParameters = ($Command.ParameterSets | Where-Object Name -eq $defaultPSetName).parameters | Where-Object Name -NotIn $common
			$otherParameters = ($Command.ParameterSets | Where-Object Name -ne $defaultPSetName).parameters | Where-Object Name -NotIn $common

			$parameters = $defaultParameters
			if ($parameters -and $otherParameters) {
				$otherParameters | ForEach-Object {
					if ($_.Name -notin $parameters.Name) {
						$parameters += $_
					}
				}
				$parameters = $parameters | Sort-Object Name
			}
		}
		else {
			$parameters = $Command.ParameterSets.Parameters | Where-Object Name -NotIn $common | Sort-Object Name -Unique
		}


		return $parameters
	}
	END { }
}

<#
.SYNOPSIS
Gets the module/snapin name and version for a command.

.DESCRIPTION
This function takes a CommandInfo object (the type that
Get-Command returns) and retuns a custom object with the
following properties:

    -- [string] $CommandName
	-- [string] $ModuleName (or PSSnapin name)
	-- [string] $ModuleVersion (or PowerShell Version)

.PARAMETER CommandInfo
Specifies a Commandinfo object, e.g. (Get-Command Get-Item).

.EXAMPLE
PS C:\> Get-CommandVersion -CommandInfo (Get-Command Get-Help)

CommandName ModuleName                Version
----------- ----------                -------
Get-Help    Microsoft.PowerShell.Core 3.0.0.0

This command gets information about a cmdlet in a PSSnapin.


.EXAMPLE
PS C:\> Get-CommandVersion -CommandInfo (Get-Command New-JobTrigger)

CommandName    ModuleName     Version
-----------    ----------     -------
New-JobTrigger PSScheduledJob 1.1.0.0

This command gets information about a cmdlet in a module.
#>
function Get-CommandVersion {
	Param
	(
		[Parameter(Mandatory = $true)]
		[System.Management.Automation.CommandInfo]
		$CommandInfo
	)

	if ((-not ((($commandModuleName = $CommandInfo.Module.Name) -and ($commandVersion = $CommandInfo.Module.Version)) -or
	(($commandModuleName = $CommandInfo.PSSnapin) -and ($commandVersion = $CommandInfo.PSSnapin.Version))))) {
		Write-Error "For $($CommandInfo.Name) :  Can't find PSSnapin/module name and version"
	}
	else {
		# "For $commandName : Module is $commandModuleName. Version is $commandVersion"
		[PSCustomObject]@{ CommandName = $CommandInfo.Name; ModuleName = $commandModuleName; Version = $commandVersion }
	}
}


if (!$RequiredVersion) {
	$RequiredVersion = (Get-Module $ModuleName -ListAvailable | Sort-Object -Property Version -Descending | Select-Object -First 1).Version
}

# Remove all versions of the module from the session. Pester can't handle multiple versions.
Get-Module $ModuleName | Remove-Module

# Import the required version
Import-Module $ModuleName -RequiredVersion $RequiredVersion -ErrorAction Stop
$ms = $null
$commands =$null
$paramBlackList = @()
if ($PSVersionTable.PSVersion -lt [Version]'5.0.0') {
	$ms = New-Object -TypeName 'Microsoft.PowerShell.Commands.ModuleSpecification' -ArgumentList $ModuleName
	$commands = Get-Command -Module $ms.Name
	$paramBlackList += 'SaveDscDependency'
}
else {
	$ms = [Microsoft.PowerShell.Commands.ModuleSpecification]@{ ModuleName = $ModuleName; RequiredVersion = $RequiredVersion }
	$commands = Get-Command -FullyQualifiedModule $ms -CommandType Cmdlet,Function,Workflow # Not alias
}

## When testing help, remember that help is cached at the beginning of each session.
## To test, restart session.

foreach ($command in $commands) {
	$commandName = $command.Name

	# Get the module name and version of the command. Used in the Describe name.
	$commandModuleVersion = Get-CommandVersion -CommandInfo $command

	# The module-qualified command fails on Microsoft.PowerShell.Archive cmdlets
	$Help = Get-Help $ModuleName\$commandName -ErrorAction SilentlyContinue
	if ($Help.Synopsis -like '*`[`<CommonParameters`>`]*') {
		$Help = Get-Help $commandName -ErrorAction SilentlyContinue
	}

	Describe "Test help for $commandName in $($commandModuleVersion.ModuleName) ($($commandModuleVersion.Version))" {

		# If help is not found, synopsis in auto-generated help is the syntax diagram
		It "should not be auto-generated" {
			# Replace pester BeLike with powershell -like as pester 3.3.9 does not support BeLike
			$Help.Synopsis -like '*`[`<CommonParameters`>`]*' | Should Be $false
		}

		# Should be a description for every function
		It "gets description for $commandName" {
			$Help.Description | Should Not BeNullOrEmpty
		}

		# Should be at least one example
		It "gets example code from $commandName" {
			($Help.Examples.Example | Select-Object -First 1).Code | Should Not BeNullOrEmpty
		}

		# Should be at least one example description
		It "gets example help from $commandName" {
			($Help.Examples.Example.Remarks | Select-Object -First 1).Text | Should Not BeNullOrEmpty
		}

		Context "Test parameter help for $commandName" {

			$Common = 'Debug', 'ErrorAction', 'ErrorVariable', 'InformationAction', 'InformationVariable', 'OutBuffer', 'OutVariable',
			'PipelineVariable', 'Verbose', 'WarningAction', 'WarningVariable'

			# Get parameters. When >1 parameter with same name,
			# get parameter from the default parameter set, if any.
			$parameters = Get-ParametersDefaultFirst -Command $command

			$parameterNames = $parameters.Name
			$HelpParameterNames = $Help.Parameters.Parameter.Name | Sort-Object -Unique

			foreach ($parameter in $parameters) {
				if ($parameter -in $paramBlackList) {
					continue
				}
				$parameterName = $parameter.Name
				$parameterHelp = $Help.parameters.parameter | Where-Object Name -EQ $parameterName

				# Should be a description for every parameter
				It "gets help for parameter: $parameterName : in $commandName" {
					# `$parameterHelp.Description.Text | Should Not BeNullOrEmpty` fails for -Settings paramter
					# without explicit [string] casting on the Text property
					[string]::IsNullOrEmpty($parameterHelp.Description.Text) | Should Be $false
				}

				# Required value in Help should match IsMandatory property of parameter
				It "help for $parameterName parameter in $commandName has correct Mandatory value" {
					$codeMandatory = $parameter.IsMandatory.toString()
					$parameterHelp.Required | Should Be $codeMandatory
				}

				# Parameter type in Help should match code
				It "help for $commandName has correct parameter type for $parameterName" {
					$codeType = $parameter.ParameterType.Name
					# To avoid calling Trim method on a null object.
					$helpType = if ($parameterHelp.parameterValue) { $parameterHelp.parameterValue.Trim() }
					$helpType | Should be $codeType
				}
			}

			foreach ($helpParm in $HelpParameterNames) {
				if ($helpParm -in $paramBlackList) {
					continue
				}
				# Shouldn't find extra parameters in help.
				It "finds help parameter in code: $helpParm" {
					$helpParm -in $parameterNames | Should Be $true
				}
			}
		}
	}
}