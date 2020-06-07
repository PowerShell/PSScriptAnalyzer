# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

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

$RequiredVersion = (Get-Command Invoke-ScriptAnalyzer).Module.Version

$ms = $null
$commands = $null
$paramBlackList = @(
	'AttachAndDebug' # Reason: When building with DEGUG configuration, an additional parameter 'AttachAndDebug' will be added to Invoke-ScriptAnalyzer and Invoke-Formatter, but there is no Help for those, as they are not intended for production usage.
)
[string] $ModuleName = 'PSScriptAnalyzer'
if ($PSVersionTable.PSVersion -lt '5.0') {
	$ms = New-Object -TypeName 'Microsoft.PowerShell.Commands.ModuleSpecification' -ArgumentList $ModuleName
	$commands = Get-Command -Module $ms.Name
}
else {
	$ms = [Microsoft.PowerShell.Commands.ModuleSpecification]@{ ModuleName = $ModuleName; RequiredVersion = $RequiredVersion }
	$commands = Get-Command -FullyQualifiedModule $ms
}

$testCases = $commands.ForEach{
	@{
		Command              = $_
		CommandName          = $_.Name
		CommandModuleVersion = Get-CommandVersion -CommandInfo $_
		Help                 = & {
			$Help = Get-Help "${ModuleName}\$($_.Name)" -ErrorAction SilentlyContinue
			if ($Help.Synopsis -like '*`[`<CommonParameters`>`]*')
			{
				$Help = Get-Help $commandName -ErrorAction SilentlyContinue
			}
			$Help
		}
	}
}


BeforeAll {
	$paramBlackList = @(
		'AttachAndDebug' # Reason: When building with DEGUG configuration, an additional parameter 'AttachAndDebug' will be added to Invoke-ScriptAnalyzer and Invoke-Formatter, but there is no Help for those, as they are not intended for production usage.
	)
	if ($PSVersionTable.PSVersion -lt '5.0') {
		$paramBlackList += 'SaveDscDependency'
	}
}


Describe "Cmdlet help" {

	# If help is not found, synopsis in auto-generated help is the syntax diagram
	It "should not be auto-generated" -TestCases $testCases {
		# Replace pester BeLike with powershell -like as pester 3.3.9 does not support BeLike
		$Help.Synopsis -like '*`[`<CommonParameters`>`]*' | Should -BeFalse
	}

	# Should be a description for every function
	It "gets description for <CommandName>" -TestCases $testCases {
		$Help.Description | Should -Not -BeNullOrEmpty
	}

	# Should be at least one example
	It "gets example code from <CommandName>" -TestCases $testCases {
		($Help.Examples.Example | Select-Object -First 1).Code | Should -Not -BeNullOrEmpty
	}

	# Should be at least one example description
	It "gets example help from $commandName" -TestCases $testCases {
		($Help.Examples.Example.Remarks | Select-Object -First 1).Text | Should -Not -BeNullOrEmpty
	}
}

Describe 'Cmdlet parameter help' {
	BeforeAll {
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
				$Common = 'Debug', 'ErrorAction', 'ErrorVariable', 'InformationAction', 'InformationVariable', 'OutBuffer', 'OutVariable', 'PipelineVariable', 'Verbose', 'WarningAction', 'WarningVariable', 'WhatIf', 'Confirm'
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
	}
	It "Parameter help for <CommandName>" -TestCases $testCases {
		# Get parameters. When >1 parameter with same name,
		# get parameter from the default parameter set, if any.
		$parameters = Get-ParametersDefaultFirst -Command $command

		$parameterNames = $parameters.Name
		$HelpParameterNames = $Help.Parameters.Parameter.Name | Sort-Object -Unique

		foreach ($parameter in $parameters) {
			if ($parameter.Name -in $paramBlackList) {
				continue
			}
			$parameterName = $parameter.Name
			$parameterHelp = $Help.parameters.parameter | Where-Object Name -EQ $parameterName

			# Should be a description for every parameter
			# It "gets help for parameter: $parameterName : in $commandName" {
				# `$parameterHelp.Description.Text | Should -Not -BeNullOrEmpty` fails for -Settings paramter
				# without explicit [string] casting on the Text property
				$parameterHelp.Description.Text | Should -Not -BeNullOrEmpty -Because "Parameter '$parameterName' of command '$CommandName' should have a Description"
			# }

			# Required value in Help should match IsMandatory property of parameter
			$codeMandatory = $parameter.IsMandatory.toString()
			$parameterHelp.Required | Should -Be $codeMandatory -Because "Parameter '$parameterName' of command '$CommandName should have the correct IsMandatory attribute"

			# Parameter type in Help should match code
			$codeType = $parameter.ParameterType.Name
			# To avoid calling Trim method on a null object.
			$helpType = if ($parameterHelp.parameterValue) { $parameterHelp.parameterValue.Trim() }
			$helpType | Should -Be $codeType -Because "help for $commandName has correct parameter type for $parameterName"
		}

		foreach ($helpParam in $HelpParameterNames) {
			if ($helpParam -in $paramBlackList) {
				continue
			}
			$helpParam -in $parameterNames | Should -BeTrue -Because "There should be no extra parameters in help. '$helpParam' was not in '$parameterNames'"
		}
	}
}
