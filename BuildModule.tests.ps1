# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# these are tests for the build module

import-module -force "./build.psm1"
Describe "Build Module Tests" {
    Context "Global.json" {
        BeforeAll {
            $globalJson = Get-Content (Join-Path $PSScriptRoot global.json) | ConvertFrom-Json
            $expectedVersion = $globalJson.sdk.version
            $result = Get-GlobalJsonSdkVersion
        }
        $propertyTestcases = @{ Name = "Major"; Type = "System.Int32" },
            @{ Name = "Minor"; Type = "System.Int32" },
            @{ Name = "Patch"; Type = "System.Int32" },
            @{ Name = "PrereleaseLabel"; Type = "System.String" }
        It "Get-GlobalJsonSdkVersion returns a portable version object with property '<Name>' with type '<Type>'" -TestCases $propertyTestcases {
            param ( $Name, $Type )
            $result.psobject.properties[$Name] | Should -BeOfType [System.Management.Automation.PSNoteProperty]
            $result.psobject.properties[$Name].TypeNameOfValue | Should -Be $Type
        }
        It "Can retrieve the version from global.json" {
            $result = Get-GlobalJsonSdkVersion
            $resultString = "{0}.{1}.{2}" -f $result.Major,$result.Minor,$result.Patch
            if ( $result.prereleasestring ) { $resultString += "-" + $result.prereleasestring }
            $resultString | Should -Be $expectedVersion
        }
    }
    Context "Test-SuiteableDotnet" {
        It "Test-SuitableDotnet should return true when the expected version matches the installed version" {
            Test-SuitableDotnet -availableVersions 2.1.2 -requiredVersion 2.1.2 | Should -Be $true
        }
        It "Test-SuitableDotnet should return true when the expected version matches the available versions" {
            Test-SuitableDotnet -availableVersions "2.1.1","2.1.2","2.1.3" -requiredVersion 2.1.2 | Should -Be $true
        }
        It "Test-SuitableDotnet should return false when the expected version does not match an available" {
            Test-SuitableDotnet -availableVersions "2.2.100","2.2.300" -requiredVersion 2.2.200 | Should -Be $false
        }
        It "Test-SuitableDotnet should return false when the expected version does not match an available" {
            Test-SuitableDotnet -availableVersions "2.2.100","2.2.300" -requiredVersion 2.2.105 | Should -Be $false
        }
        It "Test-SuitableDotnet should return true when the expected version matches an available" {
            Test-SuitableDotnet -availableVersions "2.2.150","2.2.300" -requiredVersion 2.2.105 | Should -Be $true
        }
        It "Test-SuitableDotnet should return false when the expected version does not match an available" {
            Test-SuitableDotnet -availableVersions "2.2.400","2.2.401","2.2.405" -requiredVersion "2.2.410" | Should -Be $false
        }
    }

    Context "Test-DotnetInstallation" {
        BeforeAll {
            $availableVersions = ConvertTo-PortableVersion -strVersion "2.2.400","2.2.401","2.2.405"
            $foundVersion = ConvertTo-PortableVersion -strVersion 2.2.402
            $missingVersion = ConvertTo-PortableVersion -strVersion 2.2.410
        }

        It "Test-DotnetInstallation finds a good version" {
            Mock Get-InstalledCLIVersion { return $availableVersions }
            Mock Get-GlobalJSonSdkVersion { return $foundVersion }
            $result = Test-DotnetInstallation -requestedVersion (Get-GlobalJsonSdkVersion) -installedVersions (Get-InstalledCLIVersion)
            Assert-MockCalled "Get-InstalledCLIVersion" -Times 1
            Assert-MockCalled "Get-GlobalJsonSdkVersion" -Times 1
            $result | Should -Be $true
        }

        It "Test-DotnetInstallation cannot find a good version should return false" {
            Mock Get-InstalledCLIVersion { return $availableVersions }
            Mock Get-GlobalJSonSdkVersion { return $missingVersion }
            $result = Test-DotnetInstallation -requestedVersion (Get-GlobalJsonSdkVersion) -installedVersions (Get-InstalledCLIVersion)
            Assert-MockCalled "Get-InstalledCLIVersion" -Times 1
            Assert-MockCalled "Get-GlobalJsonSdkVersion" -Times 1
            $result | Should -Be $false
        }
    }

    Context "Receive-DotnetInstallScript" {
        Mock -ModuleName Build Receive-File { new-item -type file TestDrive:/dotnet-install.sh }
        It "Downloads the proper file" {
            try {
                push-location TestDrive:
                Receive-DotnetInstallScript -forceNonWindows
                "TestDrive:/dotnet-install.sh" | Should -Exist
            }
            finally {
                Pop-Location
            }
        }

    }
}
