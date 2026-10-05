# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'CommandInfoCache' {
    BeforeEach {
        $cacheType = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper].Assembly.GetType(
            'Microsoft.Windows.PowerShell.ScriptAnalyzer.CommandInfoCache', $true)
        $cache = [Activator]::CreateInstance($cacheType)
    }

    AfterEach {
        $cache.Dispose()
    }

    It 'Returns the cached command for differently cased names and equivalent command types' {
        $command = $cache.GetCommandInfo('Get-Item', $null, $false)
        $command | Should -Not -BeNullOrEmpty
        $cached = $cache.GetCommandInfo('gET-iTEM', [System.Management.Automation.CommandTypes]::All, $false)
        [object]::ReferenceEquals($command, $cached) | Should -BeTrue
    }

    It 'Keeps command-type filters separate from an existing cache entry' {
        $cache.GetCommandInfo('Get-Item', [System.Management.Automation.CommandTypes]::Function, $false) |
            Should -BeNullOrEmpty
        $command = $cache.GetCommandInfo('Get-Item', [System.Management.Automation.CommandTypes]::Cmdlet, $false)
        $command.CommandType | Should -Be ([System.Management.Automation.CommandTypes]::Cmdlet)
        $cache.GetCommandInfo('GET-ITEM', [System.Management.Automation.CommandTypes]::Function, $false) |
            Should -BeNullOrEmpty
    }

    It 'Uses ordinal case-insensitive lookup independently of the current culture' {
        $originalCulture = [System.Threading.Thread]::CurrentThread.CurrentCulture
        try {
            $command = $cache.GetCommandInfo('Get-Item', $null, $false)
            [System.Threading.Thread]::CurrentThread.CurrentCulture = [System.Globalization.CultureInfo]::GetCultureInfo('tr-TR')
            $cached = $cache.GetCommandInfo('get-item', $null, $false)
            [object]::ReferenceEquals($command, $cached) | Should -BeTrue
        } finally {
            [System.Threading.Thread]::CurrentThread.CurrentCulture = $originalCulture
        }
    }
}
