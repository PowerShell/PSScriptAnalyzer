# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Platform data collection" {
    It "Reports the .NET runtime as Core on PowerShell 6 and later" -Skip:($PSVersionTable.PSVersion.Major -lt 6) {
        (Get-PSCompatibilityPlatformData).Dotnet.Runtime | Should -Be 'Core'
    }
}

Describe "Module data collection" {
    It "Collects Microsoft.PowerShell.Diagnostics commands despite its type data already being loaded" -Skip:(-not $IsWindows -or $PSVersionTable.PSVersion.Major -lt 7) {
        $pwsh = [powershell]::Create()
        try {
            $psVersion = [Microsoft.PowerShell.CrossCompatibility.PowerShellVersion]::Create($PSVersionTable.PSVersion)
            $collector = [Microsoft.PowerShell.CrossCompatibility.Collection.PowerShellDataCollector+Builder]::new().Build($pwsh, $psVersion)
            $module = Get-Module -ListAvailable -Name (Join-Path $PSHOME 'Modules/Microsoft.PowerShell.Diagnostics')

            $err = $null
            $moduleData = $collector.LoadAndGetModuleData($module, [ref]$err)

            $err | Should -BeNullOrEmpty
            $moduleData.Item3.Cmdlets.Keys | Should -Contain 'Get-WinEvent'
        }
        finally {
            $pwsh.Dispose()
        }
    }
}
