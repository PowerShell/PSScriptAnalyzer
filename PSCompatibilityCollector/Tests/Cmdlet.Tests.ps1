# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Write-Verbose -Verbose $env:PSModulePath

$script:currentMachineProfile = New-PSCompatibilityProfile -PassThru

Describe "Assert-PSCompatibilityProfileIsValid" {
    It "Accepts a good profile" {
        Assert-PSCompatibilityProfileIsValid -CompatibilityProfile $script:currentMachineProfile
        $? | Should -BeTrue
    }
}

Describe "JSON cmdlets" {
    It "Serializes a compatibility profile to JSON and back again" {
        $script:currentMachineProfile |
            ConvertTo-PSCompatibilityJson |
            ConvertFrom-PSCompatibilityJson |
            Assert-PSCompatibilityProfileIsValid

        $? | Should -BeTrue
    }
}

Describe "Platform data and naming" {
    It "Collects platform data properly" {
        $platformData = Get-PSCompatibilityPlatformData

        $platformData.PowerShell.Version.Major | Should -Be $PSVersionTable.PSVersion.Major

        if ($PSEdition -eq 'Core')
        {
            $platformData.Dotnet.Edition | Should -Be 'Core'
        }
        else
        {
            $platformData.Dotnet.Edition | Should -Be 'Framework'
        }

        if ($IsLinux)
        {
            $platformData.OperatingSystem.Family | Should -Be 'Linux'
        }
        elseif ($IsMacOS)
        {
            $platformData.OperatingSystem.Family | Should -Be 'MacOS'
        }
        else
        {
            $platformData.OperatingSystem.Family | Should -Be 'Windows'
        }
    }

    It "Names the platform appropriately" {
        $platformName = Get-PSCompatibilityPlatformName

        $platformNameElements = $platformName -split "_"

        $platformNameElements | Should -HaveCount 6
    }
}

Describe "New-PSCompatibilityProfile" {
    It "Generates a compatibility profile" {
        $jsonPath = Join-Path $TestDrive "profile.json"
        New-PSCompatibilityProfile -OutFile $jsonPath
        $json = ConvertFrom-PSCompatibilityJson -Path $jsonPath
        Assert-PSCompatibilityProfileIsValid $json
        $? | Should -BeTrue
    }
}