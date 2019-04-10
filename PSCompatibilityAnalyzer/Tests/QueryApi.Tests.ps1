# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe ".NET type with members with names differing only by name" {
    BeforeAll {
        Add-Type -TypeDefinition @'
namespace PSScriptAnalyzerTests
{
    public class QueryApiTestObject
    {
        public string JobId { get; set; }

        public object JOBID { get; set; }
    }
}
'@
    }

    It "Does not crash the query API" {
        $typeData = [Microsoft.PowerShell.CrossCompatibility.Utility.TypeDataConversion]::AssembleType([PSScriptAnalyzerTests.QueryApiTestObject])

        $typeQueryObject = New-Object 'Microsoft.PowerShell.CrossCompatibility.Query.TypeData' ('QueryApiTestObject', $typeData)

        $typeData.Instance.Properties.Count | Should -Be 2

        $typeData.Instance.Properties.ContainsKey('JobId') | Should -BeTrue
        $typeData.Instance.Properties.ContainsKey('JOBID') | Should -BeTrue
        $typeData.Instance.Properties.ContainsKey('jobid') | Should -Not -BeTrue

        $typeQueryObject.Instance.Properties.Count | Should -Be 1

        $typeQueryObject.Instance.Properties.ContainsKey('JobId') | Should -BeTrue
        $typeQueryObject.Instance.Properties.ContainsKey('JobID') | Should -BeTrue
        $typeQueryObject.Instance.Properties.ContainsKey('jobid') | Should -BeTrue
    }
}