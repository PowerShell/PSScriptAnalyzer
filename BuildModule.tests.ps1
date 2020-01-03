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
        It "Downloads the proper non-Windows file" {
            try {
                push-location TestDrive:
                Receive-DotnetInstallScript -platform NonWindows
                "TestDrive:/dotnet-install.sh" | Should -Exist
            }
            finally {
                Pop-Location
            }
        }

        Mock -ModuleName Build Receive-File { new-item -type file TestDrive:/dotnet-install.ps1 }
        It "Downloads the proper file Windows file" {
            try {
                push-location TestDrive:
                Receive-DotnetInstallScript -platform "Windows"
                "TestDrive:/dotnet-install.ps1" | Should -Exist
            }
            finally {
                Pop-Location
            }
        }

    }

    Context "Test result functions" {
        BeforeAll {
            $xmlFile = @'
﻿<?xml version="1.0" encoding="utf-8" standalone="no"?>
<test-results xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="nunit_schema_2.5.xsd" name="Pester" total="2" errors="0" failures="1" not-run="0" inconclusive="0" ignored="0" skipped="0" invalid="0" date="2019-02-19" time="11:36:56">
  <environment platform="Darwin" clr-version="Unknown" os-version="18.2.0" cwd="/Users/jimtru/src/github/forks/JamesWTruher/PSScriptAnalyzer" user="jimtru" user-domain="" machine-name="Jims-Mac-mini.guest.corp.microsoft.com" nunit-version="2.5.8.0" />
  <culture-info current-culture="en-US" current-uiculture="en-US" />
  <test-suite type="TestFixture" name="Pester" executed="True" result="Failure" success="False" time="0.0982" asserts="0" description="Pester">
    <results>
      <test-suite type="TestFixture" name="/tmp/bad.tests.ps1" executed="True" result="Failure" success="False" time="0.0982" asserts="0" description="/tmp/bad.tests.ps1">
        <results>
          <test-suite type="TestFixture" name="test function" executed="True" result="Failure" success="False" time="0.084" asserts="0" description="test function">
            <results>
              <test-case description="a passing test" name="test function.a passing test" time="0.0072" asserts="0" success="True" result="Success" executed="True" />
              <test-case description="a failing test" name="test function.a failing test" time="0.0268" asserts="0" success="False" result="Failure" executed="True">
                <failure>
                  <message>Expected 2, but got 1.</message>
                  <stack-trace>at &lt;ScriptBlock&gt;, /tmp/bad.tests.ps1: line 3
3:     It "a failing test" { 1 | Should -Be 2 }</stack-trace>
                </failure>
              </test-case>
            </results>
          </test-suite>
        </results>
      </test-suite>
    </results>
  </test-suite>
</test-results>
'@

            $xmlFile | out-file TESTDRIVE:/results.xml
            $results = Get-TestResults -logfile TESTDRIVE:/results.xml
            $failures = Get-TestFailures -logfile TESTDRIVE:/results.xml
        }

        It "Get-TestResults finds 2 results" {
            $results.Count | Should -Be 2
        }
        It "Get-TestResults finds 1 pass" {
            @($results | Where-Object -FilterScript { $_.result -eq "Success" }).Count | Should -Be 1
        }
        It "Get-TestResults finds 1 failure" {
            @($results | Where-Object -FilterScript { $_.result -eq "Failure" }).Count | Should -Be 1
        }
        It "Get-TestFailures finds 1 failure" {
            $failures.Count | Should -Be 1
        }
    }
}
