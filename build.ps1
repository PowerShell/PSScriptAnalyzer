# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[CmdletBinding(DefaultParameterSetName="BuildOne")]
param(
    [Parameter(ParameterSetName="BuildAll")]
    [switch]$All,

    [Parameter(ParameterSetName="BuildOne")]
    [ValidateSet("full", "core")]
    [string]$Framework = "core",

    [Parameter(ParameterSetName="BuildOne")]
    [ValidateSet("3","4","5")]
    [string]$PSVersion = "5",

    [Parameter(ParameterSetName="BuildOne")]
    [Parameter(ParameterSetName="BuildAll")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [Parameter(ParameterSetName="BuildDoc")]
    [switch]$Documentation,

    [Parameter(ParameterSetName='BuildAll')]
    [Parameter(ParameterSetName='BuildOne')]
    [switch]$Clobber,

    [Parameter(Mandatory=$true,ParameterSetName='Clean')]
    [switch] $Clean,

    [Parameter(Mandatory=$true,ParameterSetName='Test')]
    [switch] $Test,

    [Parameter(ParameterSetName='Test')]
    [switch] $InProcess
)

END {
    Import-Module -Force (Join-Path $PSScriptRoot build.psm1)
    if ( $Clean -or $Clobber ) {
        Remove-Build
        if ( $PSCmdlet.ParameterSetName -eq "Clean" ) {
            return
        }
    }

    $setName = $PSCmdlet.ParameterSetName
    switch ( $setName ) {
        "BuildAll" {
            Build-ScriptAnalyzer -All -Configuration $Configuration
        }
        "BuildDoc" {
            Build-ScriptAnalyzer -Documentation
        }
        "BuildOne" {
            $buildArgs = @{
                Framework = $Framework
                PSVersion = $PSVersion
                Configuration = $Configuration
            }
            Build-ScriptAnalyzer @buildArgs
        }
        "Test" {
            Test-ScriptAnalyzer -InProcess:$InProcess
            return
        }
        default {
            throw "Unexpected parameter set '$setName'"
        }
    }
}
