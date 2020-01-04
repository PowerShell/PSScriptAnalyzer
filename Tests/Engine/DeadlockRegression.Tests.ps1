# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Complex file analysis" {
    It "Analyzes file successfully" {
        Invoke-ScriptAnalyzer -Path "$PSScriptRoot/DeadlockTestAssets/complex.psm1"
    }
}
