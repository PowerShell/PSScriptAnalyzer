<#	
	.NOTES
		===========================================================================
		Created with: 	SAPIEN Technologies, Inc., PowerShell Studio 2016 v5.2.119
		Created on:   	4/12/2016 1:11 PM
		Created by:   	June Blender
		Organization: 	SAPIEN Technologies, Inc
		Filename:		*.Help.Tests.ps1
		===========================================================================
	.DESCRIPTION
	Test help for a module. You can run this Tests file from any location. 
    To specify the module to test, enter values for the $ModuleName and $RequiredVersion variables.

	For a help test that is located in a module directory, use https://github.com/juneb/PesterTDD/InModule.Help.Tests.ps1
#>

Param
(
	[Parameter(Mandatory = $true)]
	[ValidateScript({ Get-Module -ListAvailable -Name $_ })]
	[string]
	$ModuleName = 'PSScriptAnalyzer',
	
	[Parameter(Mandatory = $false)]
	[System.Version]
	$RequiredVersion
)

if (!$RequiredVersion)
{
	$RequiredVersion = (Get-Module $ModuleName -ListAvailable | Sort-Object -Property Version -Descending | Select-Object -First 1).Version
}

# Remove all versions of the module from the session. Pester can't handle multiple versions.
Get-Module $ModuleName | Remove-Module

# Import the required version
Import-Module $ModuleName -RequiredVersion $RequiredVersion -ErrorAction Stop
$ms = [Microsoft.PowerShell.Commands.ModuleSpecification]@{ ModuleName = $ModuleName; RequiredVersion = $RequiredVersion }
$commands = Get-Command -FullyQualifiedModule $ms -CommandType Cmdlet, Function, Workflow  # Not alias

## When testing help, remember that help is cached at the beginning of each session.
## To test, restart session.

foreach ($command in $commands)
{
	$commandName = $command.Name
	
	# The module-qualified command fails on Microsoft.PowerShell.Archive cmdlets
	$Help = Get-Help $commandName -ErrorAction SilentlyContinue
	
	Describe "Test help for $commandName" {
		
		# If help is not found, synopsis in auto-generated help is the syntax diagram
		It "should not be auto-generated" {
			$Help.Synopsis | Should Not BeLike '*`[`<CommonParameters`>`]*'
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
			
			$parameters = $command.ParameterSets.Parameters | Sort-Object -Property Name -Unique | Where-Object { $_.Name -notin $common }
			$parameterNames = $parameters.Name
			$HelpParameterNames = $Help.Parameters.Parameter.Name | Sort-Object -Unique
			
			foreach ($parameter in $parameters)
			{
				$parameterName = $parameter.Name				
				$parameterHelp = $Help.parameters.parameter | Where-Object Name -EQ $parameterName 
				
				# Should be a description for every parameter
				It "gets help for parameter: $parameterName : in $commandName" {
					$parameterHelp.Description.Text | Should Not BeNullOrEmpty
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
			
			foreach ($helpParm in $HelpParameterNames)
			{
				# Shouldn't find extra parameters in help.
				It "finds help parameter in code: $helpParm" {
					$helpParm -in $parameterNames | Should Be $true
				}
			}			
		}
	}
}