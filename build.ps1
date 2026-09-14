# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[CmdletBinding(DefaultParameterSetName="BuildOne")]
param(
    [Parameter(ParameterSetName="BuildAll")]
    [switch]$All,

    [Parameter(ParameterSetName="BuildOne")]
    [ValidateSet(5, 7)]
    [int]$PSVersion = $PSVersionTable.PSVersion.Major,

    [Parameter(ParameterSetName="BuildOne")]
    [Parameter(ParameterSetName="BuildAll")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    # For building documentation only
    # or re-building it since docs gets built automatically only the first time
    [Parameter(ParameterSetName="BuildDocumentation")]
    [switch]$Documentation,

    [Parameter(ParameterSetName='BuildAll')]
    [Parameter(ParameterSetName='BuildOne')]
    [switch]$Clobber,

    [Parameter(Mandatory=$true,ParameterSetName='Clean')]
    [switch] $Clean,

    [Parameter(Mandatory=$true,ParameterSetName='Test')]
    [switch] $Test,

    [Parameter(ParameterSetName='Test')]
    [switch] $InProcess,
    [string] $WithPowerShell,

    [Parameter(ParameterSetName='BuildAll')]
    [switch] $Catalog,

    [Parameter(ParameterSetName='Package')]
    [switch] $BuildNupkg

)

BEGIN {
    $verboseWanted = $false
    if ( $PSBoundParameters['Verbose'] ) {
        $verboseWanted = $PSBoundParameters['Verbose'].ToBool()
    }
}

END {
    Import-Module -Force (Join-Path $PSScriptRoot build.psm1) -verbose:$false
    if ( $Clean -or $Clobber ) {
        Remove-Build -verbose:$false
        if ( $PSCmdlet.ParameterSetName -eq "Clean" ) {
            return
        }
    }

    $setName = $PSCmdlet.ParameterSetName
    switch ( $setName ) {
        "BuildAll" {
            $buildArgs = @{
                All = $true
                Configuration = $Configuration
                Verbose = $verboseWanted
                Catalog = $false
            }
            if ( $Catalog ) {
                $buildArgs['Catalog'] = $true
            }
            Start-ScriptAnalyzerBuild @buildArgs
        }
        "BuildDocumentation" {
            Start-ScriptAnalyzerBuild -Documentation -Verbose:$Verbose
        }
        "BuildOne" {
            $buildArgs = @{
                PSVersion = $PSVersion
                Configuration = $Configuration
            }
            Start-ScriptAnalyzerBuild @buildArgs
        }
        "Package" {
            Start-CreatePackage
        }
        "Test" {
            $testArgs = @{
                InProcess = $InProcess
                WithPowerShell = $WithPowerShell
                Verbose = $verboseWanted
            }
            Test-ScriptAnalyzer @testArgs
            return
        }
        default {
            throw "Unexpected parameter set '$setName'"
        }
    }
}
