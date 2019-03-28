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

        $typeData.Instance.Properties.Count | Should -Be 1
        $typeData.Instance.Properties.Keys | Should -Contain 'jobid'
    }
}