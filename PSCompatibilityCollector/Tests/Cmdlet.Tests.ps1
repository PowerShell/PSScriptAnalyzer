# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# TODO: These tests don't work in-process; they load too much and in AppVeyor, the module doesn't load properly.
#       They have been marked pending until we can work out how to run them and still merge their results into the XML.

return

Describe "PSCompatiblityCollector cmdlets" {
    BeforeAll {
        $compatModulePath = (Get-Module -ListAvailable 'PSCompatibilityCollector')[0].Path
        $pwshName = (Get-Process -Id $PID).Path
        $profileLocation = Join-Path $TestDrive 'profile.json'

        & $pwshName -Command "Import-Module '$compatModulePath'; New-PSCompatibilityProfile -OutFile '$profileLocation'"

        $currentMachineProfile = ConvertFrom-PSCompatibilityJson -Path $profileLocation
    }

    Context "Assert-PSCompatibilityProfileIsValid" {
        It "Accepts a good profile" {
            Assert-PSCompatibilityProfileIsValid -CompatibilityProfile $currentMachineProfile
            $? | Should -BeTrue
        }
    }

    Context "JSON cmdlets" {
        It "Serializes a compatibility profile to JSON and back again" {
            $currentMachineProfile |
                ConvertTo-PSCompatibilityJson |
                ConvertFrom-PSCompatibilityJson |
                Assert-PSCompatibilityProfileIsValid

            $? | Should -BeTrue
        }
    }

    Context "Platform data and naming" {
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
}
