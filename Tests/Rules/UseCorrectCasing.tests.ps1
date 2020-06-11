# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "UseCorrectCasing" {
    It "corrects case of simple cmdlet" {
        Invoke-Formatter 'get-childitem' | Should -Be 'Get-ChildItem'
    }

    It "corrects case of fully qualified cmdlet" {
        Invoke-Formatter 'Microsoft.PowerShell.management\get-childitem' | Should -Be 'Microsoft.PowerShell.Management\Get-ChildItem'
    }

    It "corrects case of of cmdlet inside interpolated string" {
        Invoke-Formatter '"$(get-childitem)"' | Should -Be '"$(get-childitem)"'
    }

    It "Corrects alias correctly" {
        Invoke-Formatter 'Gci' | Should -Be 'gci'
        Invoke-Formatter '?' | Should -Be '?'
    }

    It "Does not corrects applications on the PATH" -Skip:($IsLinux -or $IsMacOS) {
        Invoke-Formatter 'Cmd' | Should -Be 'Cmd'
        Invoke-Formatter 'MORE' | Should -Be 'MORE'
    }

    It "Preserves extension of applications on Windows" -Skip:($IsLinux -or $IsMacOS) {
        Invoke-Formatter 'cmd.exe' | Should -Be 'cmd.exe'
        Invoke-Formatter 'more.com' | Should -Be 'more.com'
    }

    It "Preserves full application path" {
        if ($IsLinux -or $IsMacOS) {
            $applicationPath = '. /bin/ls'
        }
        else {
            $applicationPath = "${env:WINDIR}\System32\cmd.exe"
        }
        Invoke-Formatter ". $applicationPath" | Should -Be ". $applicationPath"
    }

    It "Corrects case of script function" {
        function Invoke-DummyFunction { }
        Invoke-Formatter 'invoke-dummyFunction' | Should -Be 'Invoke-DummyFunction'
    }

    It "Preserves script path" {
        $path = Join-Path $TestDrive "$([guid]::NewGuid()).ps1"
        New-Item -ItemType File -Path $path
        $scriptDefinition = ". $path"
        Invoke-Formatter $scriptDefinition | Should -Be $scriptDefinition
    }

    It "Preserves UNC script path" -Skip:($IsLinux -or $IsMacOS) {
        $uncPath = [System.IO.Path]::Combine("\\$(HOSTNAME.EXE)\C$\", $TestDrive, "$([guid]::NewGuid()).ps1")
        New-Item -ItemType File -Path $uncPath
        $scriptDefinition = ". $uncPath"
        Invoke-Formatter $scriptDefinition | Should -Be $scriptDefinition
    }

    It "Corrects parameter casing" {
        function Invoke-DummyFunction ($ParameterName) { }

        Invoke-Formatter 'Invoke-DummyFunction -parametername $parameterValue' |
            Should -Be 'Invoke-DummyFunction -ParameterName $parameterValue'
        Invoke-Formatter 'Invoke-DummyFunction -parametername:$parameterValue' |
            Should -Be 'Invoke-DummyFunction -ParameterName:$parameterValue'
        Invoke-Formatter 'Invoke-DummyFunction -parametername: $parameterValue' |
            Should -Be 'Invoke-DummyFunction -ParameterName: $parameterValue'
    }

    It "Should not throw when using parameter name that does not exist" {
        Invoke-Formatter 'Get-Process -NonExistingParameterName' -ErrorAction Stop
    }

}
